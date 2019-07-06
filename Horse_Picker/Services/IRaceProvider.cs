using System;

namespace Horse_Picker.Services
{
    public interface IRaceProvider
    {
        DateTime RaceDate { get; set; }
        string RaceNo { get; set; }
        string City { get; set; }
        string Category { get; set; }
        string Distance { get; set; }
    }
}