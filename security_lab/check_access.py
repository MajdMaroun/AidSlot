"""Bounded authorization check for a local AidSlot lab with synthetic records.

Set AIDSLOT_BASE_URL=http://127.0.0.1:PORT, AIDSLOT_A_EMAIL,
AIDSLOT_A_PASSWORD, AIDSLOT_A_CAMPAIGN_ID, AIDSLOT_B_CAMPAIGN_ID,
and AIDSLOT_B_CANARY (a synthetic recipient name unique to B).
"""

import getpass
import html.parser
import http.cookiejar
import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request


class TokenParser(html.parser.HTMLParser):
    def __init__(self):
        super().__init__()
        self.token = None

    def handle_starttag(self, tag, attrs):
        if tag != "input":
            return
        fields = dict(attrs)
        if fields.get("name") == "__RequestVerificationToken":
            self.token = fields.get("value")


def fetch(opener, url, data=None):
    try:
        with opener.open(url, data=data, timeout=5) as response:
            return response.status, response.read(500_000).decode("utf-8", "replace"), response.url
    except urllib.error.HTTPError as error:
        return error.code, error.read(500_000).decode("utf-8", "replace"), error.url


def main():
    base = os.environ.get("AIDSLOT_BASE_URL", "").rstrip("/")
    parsed = urllib.parse.urlparse(base)
    if parsed.scheme not in ("http", "https") or parsed.hostname not in ("localhost", "127.0.0.1", "::1") or parsed.port is None or parsed.username or parsed.password or parsed.path or parsed.query:
        raise ValueError("Only an explicit localhost URL with a port is allowed")

    email = os.environ["AIDSLOT_A_EMAIL"]
    own_id = int(os.environ["AIDSLOT_A_CAMPAIGN_ID"])
    other_id = int(os.environ["AIDSLOT_B_CAMPAIGN_ID"])
    canary = os.environ["AIDSLOT_B_CANARY"]
    if own_id <= 0 or other_id <= 0 or own_id == other_id or len(canary) < 8:
        raise ValueError("Use two distinct positive campaign IDs and a unique synthetic canary")
    password = os.environ.get("AIDSLOT_A_PASSWORD") or getpass.getpass("Local lab password for A: ")

    # Never follow a redirect outside the explicitly authorized local origin.
    class LocalRedirect(urllib.request.HTTPRedirectHandler):
        def redirect_request(self, request, fp, code, msg, headers, newurl):
            target = urllib.parse.urlparse(newurl)
            if (target.scheme, target.hostname, target.port) != (parsed.scheme, parsed.hostname, parsed.port):
                raise ValueError("Redirect outside the local lab")
            return super().redirect_request(request, fp, code, msg, headers, newurl)

    opener = urllib.request.build_opener(LocalRedirect(), urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
    status, page, _ = fetch(opener, base + "/Account/Login")
    parser = TokenParser()
    parser.feed(page)
    if status != 200 or not parser.token:
        raise RuntimeError("Login page or antiforgery token unavailable")
    form = urllib.parse.urlencode({"Email": email, "Password": password, "__RequestVerificationToken": parser.token}).encode()
    status, _, final_url = fetch(opener, base + "/Account/Login", form)
    if "/Account/Login" in final_url or status != 200:
        raise RuntimeError("Login failed; no security conclusion is possible")

    results = []
    for label, campaign_id in (("own", own_id), ("other", other_id)):
        status, body, final_url = fetch(opener, f"{base}/Campaign/Recipients/{campaign_id}")
        results.append({"case": label, "status": status, "redirected_to_login": "/Account/Login" in final_url,
                        "contains_b_canary": canary in body, "response_bytes": len(body.encode())})

    own, other = results
    if own["status"] != 200 or own["redirected_to_login"]:
        decision = "INCONCLUSIVE: legitimate access failed"
    elif other["contains_b_canary"] and other["status"] == 200:
        decision = "VULNERABLE: cross-organization synthetic recipient disclosed"
    elif other["status"] in (403, 404) and not other["contains_b_canary"]:
        decision = "PROTECTED: cross-organization campaign inaccessible"
    else:
        decision = "INCONCLUSIVE: inspect unexpected response"
    print(json.dumps({"target": "local AidSlot lab", "decision": decision, "evidence": results,
                      "remediation": "Check campaign OrganizationId against the authenticated user's OrganizationId in every read and write action."}, indent=2))
    return 1 if "VULNERABLE" in decision else 0 if "PROTECTED" in decision else 2


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (KeyError, ValueError, RuntimeError, urllib.error.URLError) as error:
        print(f"INCONCLUSIVE: {error}", file=sys.stderr)
        sys.exit(2)
