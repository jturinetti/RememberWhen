using System;
using System.Collections.Generic;

namespace RememberWhen.Lambda.Services
{
    public interface IMemoryService
    {
        string RetrieveRandomMemory();
    }

    public class StaticMemoryService : IMemoryService
    {
        private readonly IList<string> _memoryList;

        public StaticMemoryService()
        {
            _memoryList = new List<string>
            {
                "Remember that time we got married?",
                "Remember that time we took a trip to Europe to see Italy and Greece?",
                "Remember the night we met at Kara's party?",
                "Remember the chair in Hawaii?",
                "Remember that time at our friend's wedding when I made you walk to the restroom by yourself?"
            };
        }

        public string RetrieveRandomMemory()
        {
            var randomizer = new Random(DateTime.UtcNow.DayOfYear + DateTime.UtcNow.Second); // this could be better
            var randomIndex = randomizer.Next(_memoryList.Count);

            return _memoryList[randomIndex];
        }
    }
}
