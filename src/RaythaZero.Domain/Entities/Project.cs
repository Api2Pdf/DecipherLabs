using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RaythaZero.Domain.Entities;

public class Project : BaseAuditableEntity
{
    public string? Label { get; set; }
    public bool? IsArchived { get; set; } = false;
    public string? _ProjectData { get; set; }
    
    private ProjectLevelInfo _projectData;

    [NotMapped]
    public ProjectLevelInfo ProjectData 
    { 
        get 
        { 
            if (_projectData == null)
            {
                _projectData = JsonSerializer.Deserialize<ProjectLevelInfo>(_ProjectData ?? JsonSerializer.Serialize(new ProjectLevelInfo()));
            }
            return _projectData; 
        } 
        set
        {
            _projectData = value;
            _ProjectData = JsonSerializer.Serialize(value);
        }
    } 

    public BlsData Bls { get; set; } = new();
}

public record ProjectLevelInfo
{
    public string DsipProposalNumber { get; set; } = string.Empty;
    public string TypeOfProposal { get; set; } = string.Empty;
    public string TopicNumber { get; set; } = string.Empty;
    public IEnumerable<string> OtherDirectCostSelections { get; set; } = new List<string>();
    public IEnumerable<string> Resumes { get; set; } = new List<string>();
    public Travel Travel { get; set; } = new Travel();
    public Materials Materials { get; set; } = new Materials();
    public Supplies Supplies { get; set; } = new Supplies();
    public Equipment Equipment { get; set; } = new Equipment();
    public OtherDirectCosts OtherDirectCosts { get; set; } = new OtherDirectCosts();
    public Consultant Consultant { get; set; } = new Consultant();
    public Subcontractor Subcontractor { get; set; } = new Subcontractor();
    public string truncated_project_description { get; set; } = string.Empty;
}


public abstract record AbstractSubtier
{
    public string Description { get; set; } = string.Empty;
    public string DescriptionMediaId { get; set; } = string.Empty;
}

public record Travel : AbstractSubtier
{
    public string Description { get; set; } = string.Empty;
    public int NumberOfTrips { get; set; } = 0;
    public int NumberOfTravelers { get; set; } = 0;
    public string EndUserLocationState { get; set; } = string.Empty;
    public string EndUserLocationCity { get; set; } = string.Empty;
    public bool HasSubcontractorLocation { get; set; } = false;
    public string SubcontractorLocationState { get; set; } = string.Empty;
    public string SubcontractorLocationCity { get; set; } = string.Empty;
    public bool UseRideshare { get; set; } = false;
    public bool UseRentalCar { get; set; } = false;
    public FinalPackage.TravelCostInfo TravelCost { get; set; } = new();
    public string TravelCostWriteup { get; set; } = string.Empty;
    public string FlightCostWriteup { get; set; } = string.Empty;
}

public record Subcontractor : AbstractSubtier
{
    public string Url { get; set; } = string.Empty;
}

public record Consultant : AbstractSubtier
{
    public string Url { get; set; } = string.Empty;
}

public record Supplies : AbstractSubtier
{
}

public record Equipment : AbstractSubtier
{
}

public record OtherDirectCosts : AbstractSubtier
{
}

public record Materials : AbstractSubtier
{
}


public class Topic
{
    public int item_number { get; set; }
    public string topic_number { get; set; }
    public string topic_title { get; set; }
    public string open_date { get; set; }
    public string close_date { get; set; }
    public bool phase_1_available { get; set; }
    public string phase_1_period_of_performance { get; set; }
    public bool phase_2_available { get; set; }
    public string phase_2_period_of_performance { get; set; }
    public decimal award_amount { get; set; }
    public bool itar { get; set; }
    public string tpoc_name { get; set; }
    public string tpoc_number { get; set; }
    public string tpoc_email { get; set; }
    public string tpoc2_name { get; set; }
    public string tpoc2_number { get; set; }
    public string tpoc2_email { get; set; }
    public string tpoc3_name { get; set; }
    public string tpoc3_number { get; set; }
    public string tpoc3_email { get; set; }
}

public record FinalPackage
{
    public string company_name { get; set; } = string.Empty;
    public string company_url { get; set; } = string.Empty;
    public string company_city_hq { get; set; } = string.Empty;
    public string company_state_hq { get; set; } = string.Empty;
    public bool offers_benefits { get; set; } = false;
    public string offers_benefits_description { get; set; } = string.Empty;

    public string offers_benefits_extracted { get; set; } = string.Empty;
    public string wage_rate_sheet_file_text { get; set; } = string.Empty;
    public string previous_cost_volumes_word_file_text { get; set; } = string.Empty;
    public string previous_cost_volumes_excel_file_text { get; set; } = string.Empty;
    public string balance_sheet_file_text { get; set; } = string.Empty;
    public string profit_and_loss_file_text { get; set; } = string.Empty; 
    
    public decimal fringe_rate { get; set; } = 0;
    public decimal fully_loaded_labor_amount { get; set; } = 0;

    public string fringe_rate_generated { get; set; } = string.Empty;
    
    public string topic_number { get; set; } = string.Empty;
    public Topic topic { get; set; } = new Topic();
    public string dsip_proposal_number { get; set; } = string.Empty;
    public string type_of_proposal { get; set; } = string.Empty;
    
    public IEnumerable<string> other_direct_cost_selections { get; set; } = new List<string>();
    public List<IndividualPersonFinalPackage> individuals { get; set; } = new List<IndividualPersonFinalPackage>();
    public IndividualPersonFinalPackage current_person { get; set; } = new();
    
    public Travel travel { get; set; } = new Travel();
    public Materials materials { get; set; } = new Materials();
    public Supplies supplies { get; set; } = new Supplies();
    public Equipment equipment { get; set; } = new Equipment();
    public OtherDirectCosts other_direct_costs { get; set; } = new OtherDirectCosts();
    public Consultant consultant { get; set; } = new Consultant();
    public Subcontractor subcontractor { get; set; } = new Subcontractor();

    public TravelCostInfo travel_cost { get; set; } = new TravelCostInfo();
    public string travel_cost_writeup { get; set; } = string.Empty;
    public string flight_cost_writeup { get; set; } = string.Empty;
    
    public decimal total_cost { get; set; }
    public string truncated_project_description { get; set; } = string.Empty;
    public decimal direct_labor_amount { get; set; }

    [NotMapped]
    public Dictionary<string, BlsOccupationData> bls_data { get; set; } = new();

    public record IndividualPersonFinalPackage
    {
        public string name { get; set; } = string.Empty;
        public string job_title { get; set; } = string.Empty;
        public string bls_code { get; set; } = string.Empty;
        public string grad_year { get; set; } = string.Empty;
        public string wage_rate { get; set; } = string.Empty;
        public string file_text { get; set; } = string.Empty;
    }

    public record Travel
    {
        public string description { get; set; } = string.Empty;
        public string file_text { get; set; } = string.Empty;
        public int number_of_trips { get; set; } = 0;
        public int number_of_travelers { get; set; } = 0;
        public string end_user_location_state { get; set; } = string.Empty;
        public string end_user_location_city { get; set; } = string.Empty;
        
        public bool has_subcontractor_location { get; set; } = false;
        public string subcontractor_location_state { get; set; } = string.Empty;
        public string subcontrator_location_city { get; set; } = string.Empty;
        public bool use_rideshare { get; set; } = false;
        public bool use_rental { get; set; } = false; 
    }

        public record TravelCostInfo
    {
        public decimal LodgingRate { get; set; }
        public decimal MealRate { get; set; }
        public decimal FlightCost { get; set; }
        public decimal? RentalCarCost { get; set; }
        public decimal? RideshareEstimate { get; set; }
        public decimal? SubcontractorFlightCost { get; set; }
        public decimal? SubcontractorLodgingRate { get; set; }
        public decimal? SubcontractorMealRate { get; set; }
        public int NumberOfDays { get; set; }
        public int NumberOfNights { get; set; }
        public string TimePeriod { get; set; } = string.Empty;
    }

    public record Materials
    {
        public string description { get; set; } = string.Empty;
        public string file_text { get; set; } = string.Empty;
    }

    public record Subcontractor
    {
        public string description { get; set; } = string.Empty;
        public string file_text { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
    }

    public record Consultant
    {
        public string description { get; set; } = string.Empty;
        public string file_text { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
    }

    public record Equipment
    {
        public string description { get; set; } = string.Empty;
        public string file_text { get; set; } = string.Empty;
    }

    public record OtherDirectCosts
    {
        public string description { get; set; } = string.Empty;
        public string file_text { get; set; } = string.Empty;
    }

    public record Supplies
    {
        public string description { get; set; } = string.Empty;
        public string file_text { get; set; } = string.Empty;
    }
}

[NotMapped]
public class BlsData
{
    public Dictionary<string, BlsOccupationData> OccupationData { get; set; } = new();
}

public class BlsOccupationData
{
    public string state_name { get; set; }
    public string state_acronym { get; set; }
    public string occupation_code { get; set; }
    public string occupation_title { get; set; }
    public decimal hourly_mean { get; set; }
    public decimal hourly_90_percentile { get; set; }
}
