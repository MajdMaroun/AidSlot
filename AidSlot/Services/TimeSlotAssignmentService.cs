using AidSlot.Models;

namespace AidSlot.Services;

public class TimeSlotAssignmentService
{
    private const int SlotDurationMinutes = 15;

    public void AssignSlots(
        IList<Recipient> recipients,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        if (recipients.Count == 0)
        {
            return;
        }

        if (endTime <= startTime)
        {
            throw new ArgumentException(
                "End time must be later than start time.");
        }

        var campaignDuration = endTime - startTime;

        var availableSlotCount = (int)Math.Floor(
            campaignDuration.TotalMinutes / SlotDurationMinutes);

        if (availableSlotCount < 1)
        {
            throw new ArgumentException(
                "The campaign must be at least 15 minutes long.");
        }

        var usedSlotCount = Math.Min(
            availableSlotCount,
            recipients.Count);

        var slotTimes = CreateSlotTimes(
            startTime,
            availableSlotCount,
            usedSlotCount);

        var recipientsPerSlot =
            recipients.Count / usedSlotCount;

        var extraRecipients =
            recipients.Count % usedSlotCount;

        var recipientIndex = 0;

        for (var slotIndex = 0;
             slotIndex < usedSlotCount;
             slotIndex++)
        {
            var numberInThisSlot =
                recipientsPerSlot +
                (slotIndex < extraRecipients ? 1 : 0);

            for (var personIndex = 0;
                 personIndex < numberInThisSlot;
                 personIndex++)
            {
                recipients[recipientIndex].AssignedSlotTime =
                    slotTimes[slotIndex];

                recipientIndex++;
            }
        }
    }

    private static List<TimeSpan> CreateSlotTimes(
        TimeSpan startTime,
        int availableSlotCount,
        int usedSlotCount)
    {
        var slots = new List<TimeSpan>();

        if (usedSlotCount == 1)
        {
            slots.Add(startTime);
            return slots;
        }

        if (usedSlotCount == availableSlotCount)
        {
            for (var index = 0;
                 index < availableSlotCount;
                 index++)
            {
                slots.Add(
                    startTime.Add(
                        TimeSpan.FromMinutes(
                            index * SlotDurationMinutes)));
            }

            return slots;
        }

        /*
         * If there are fewer recipients than available slots,
         * spread their groups across the campaign period.
         */
        for (var index = 0;
             index < usedSlotCount;
             index++)
        {
            var slotPosition = (int)Math.Round(
                index * (availableSlotCount - 1.0) /
                (usedSlotCount - 1));

            slots.Add(
                startTime.Add(
                    TimeSpan.FromMinutes(
                        slotPosition * SlotDurationMinutes)));
        }

        return slots;
    }
}