using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Data.Entities
{
    public class SchoolSettings
    {
        public int SchoolSettingsId { get; set; }

        [Range(0, 25)]
        public int MaxAbsencePercentage { get; set; } = 10;
    }
}
