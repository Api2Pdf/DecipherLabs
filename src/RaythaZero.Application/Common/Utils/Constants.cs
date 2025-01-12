namespace RaythaZero.Application.Common.Utils
{
    public static class Constants
    {
        public const string VALIDATION_SUMMARY = "__ValidationSummary";

        public const string AUDIT_LOG_LOGGABLE_REQUEST = "RaythaZero.Applications.Common.Models.LoggableRequest";
        public const string AUDIT_LOG_LOGGABLE_ENTITY_REQUEST = "RaythaZero.Applications.Common.Models.LoggableEntityRequest";
        
        public static List<string> GetStatesAndTerritories()
        {
            return new List<string>
            {
                "Alabama", "Alaska", "American Samoa", "Arizona", "Arkansas", 
                "California", "Colorado", "Connecticut", "Delaware", "District of Columbia",
                "Florida", "Georgia", "Guam", "Hawaii", "Idaho", "Illinois", 
                "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", 
                "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", 
                "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", 
                "New Hampshire", "New Jersey", "New Mexico", "New York", "North Carolina", 
                "North Dakota", "Northern Mariana Islands", "Ohio", "Oklahoma", 
                "Oregon", "Pennsylvania", "Puerto Rico", "Rhode Island", "South Carolina", 
                "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", 
                "Virgin Islands", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming"
            };
        }
    }
}
