using System;
using UnityEngine;
using UnityEngine.UI;

namespace EVRTH.Scripts.Utility
{
    public class DateScrubber : MonoBehaviour
    {
        public LayerPresetLoader presetLoader;
        [Header("Start Date")]
        public Date startDate;
        [Space]
        [Header("End Date")]
        public Date endDate;

        [Space]
        [Space]
        [Header("Display")]
        public Text currDateText;

        public Text nextDateText;

        public Slider scrubberSlider;

        public void SelectDate(DateTime selectedTime)
        {
            presetLoader.date.SetFromDateTime(selectedTime);
            presetLoader.ApplyPreset(presetLoader.currentPreset);
        }

        public void SelectDate(float percentage)
        {
            TimeSpan span = endDate.ToDateTime - startDate.ToDateTime;
            int days = (int) (span.TotalDays * percentage);
            presetLoader.date.SetFromDateTime(startDate.ToDateTime.AddDays(days));
            presetLoader.ApplyPreset(presetLoader.currentPreset);
            currDateText.text = string.Format("Current Date: {0:MM/dd/yyyy}", presetLoader.date.ToDateTime);
        }

        public void ShowNextDate(float percentage)
        {
            TimeSpan span = endDate.ToDateTime - startDate.ToDateTime;
            int days = (int)(span.TotalDays * percentage);
            nextDateText.text = string.Format("Current Date: {0:MM/dd/yyyy}", startDate.ToDateTime.AddDays(days));
        }

        public void GoToNextDate()
        {
            SelectDate(scrubberSlider.value);
        }
    }
}
