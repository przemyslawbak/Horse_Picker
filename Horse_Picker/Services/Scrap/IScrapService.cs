using System;
using System.Threading.Tasks;
using Horse_Picker.Models;

namespace Horse_Picker.Services.Scrap
{
    public interface IScrapService
    {
        Task<T> ScrapGenericObject<T>(int id, string jobType);
    }
}
