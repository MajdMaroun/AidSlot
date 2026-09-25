$ErrorActionPreference = 'Stop'

$base = $env:AIDSLOT_BASE_URL.TrimEnd('/')
$uri = [Uri]$base
if ($uri.Scheme -notin @('http','https') -or $uri.Host -notin @('localhost','127.0.0.1','::1') -or $uri.IsDefaultPort -or $uri.AbsolutePath -ne '/') {
    throw 'Only an explicit localhost URL with a port is allowed.'
}

$email = $env:AIDSLOT_A_EMAIL
$password = $env:AIDSLOT_A_PASSWORD
$ownId = [int]$env:AIDSLOT_A_CAMPAIGN_ID
$otherId = [int]$env:AIDSLOT_B_CAMPAIGN_ID
$canary = $env:AIDSLOT_B_CANARY
if (-not $email -or -not $password -or $ownId -le 0 -or $otherId -le 0 -or $ownId -eq $otherId -or $canary.Length -lt 8) {
    throw 'Set the A account, two distinct campaign IDs, and a unique synthetic B canary.'
}

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$login = Invoke-WebRequest -Uri "$base/Account/Login" -WebSession $session -UseBasicParsing
$inputTag = [regex]::Match($login.Content, '<input\b[^>]*\bname="__RequestVerificationToken"[^>]*>', 'IgnoreCase')
$token = [regex]::Match($inputTag.Value, '\bvalue="([^"]+)"', 'IgnoreCase')
if (-not $token.Success) { throw 'Login antiforgery token unavailable; result is inconclusive.' }

$form = @{
    Email = $email
    Password = $password
    __RequestVerificationToken = [System.Net.WebUtility]::HtmlDecode($token.Groups[1].Value)
}
$signedIn = Invoke-WebRequest -Uri "$base/Account/Login" -Method Post -Body $form -WebSession $session -UseBasicParsing
if ($signedIn.BaseResponse.ResponseUri.AbsolutePath -eq '/Account/Login') {
    throw 'Login failed; result is inconclusive.'
}

function Test-Campaign([string]$label, [int]$id) {
    try {
        $response = Invoke-WebRequest -Uri "$base/Campaign/Recipients/$id" -WebSession $session -UseBasicParsing
        $status = [int]$response.StatusCode
        $body = [string]$response.Content
        $loginRedirect = $response.BaseResponse.ResponseUri.AbsolutePath -eq '/Account/Login'
    } catch [System.Net.WebException] {
        $httpResponse = $_.Exception.Response
        if (-not $httpResponse) { throw }
        $status = [int]$httpResponse.StatusCode
        $reader = New-Object System.IO.StreamReader($httpResponse.GetResponseStream())
        $body = $reader.ReadToEnd()
        $reader.Dispose()
        $loginRedirect = $false
    }
    return [pscustomobject]@{
        Case = $label
        Status = $status
        RedirectedToLogin = $loginRedirect
        ContainsB_Canary = $body.Contains($canary)
        ResponseLength = $body.Length
    }
}

$own = Test-Campaign 'own' $ownId
$other = Test-Campaign 'other' $otherId
if ($own.Status -ne 200 -or $own.RedirectedToLogin) {
    $decision = 'INCONCLUSIVE: legitimate access failed'
} elseif ($other.Status -eq 200 -and $other.ContainsB_Canary) {
    $decision = 'VULNERABLE: cross-organization synthetic recipient disclosed'
} elseif ($other.Status -in @(403,404) -and -not $other.ContainsB_Canary) {
    $decision = 'PROTECTED: cross-organization campaign inaccessible'
} else {
    $decision = 'INCONCLUSIVE: inspect unexpected response'
}

[pscustomobject]@{
    Target = 'local AidSlot lab'
    Decision = $decision
    Evidence = @($own,$other)
    Remediation = 'Check campaign OrganizationId against the authenticated user OrganizationId on every read and write.'
} | ConvertTo-Json -Depth 4
