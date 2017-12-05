using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesParserr.Model
{
    public class MovieInfo
    {
        public string NameForExcel;
        public int Id;
        public string Name;
        public string SeasonEpisode ;
        public string ServerPath;
        public string fullName;
        public string fullNameToShow;
        public int Season;
        public int Episode;
        

        public MovieInfo(string nameExcel, int id, string name, int season, int episode, string serverPath, string fullNameShow)
        {
            NameForExcel = nameExcel;
            Id = id;
            Name = name;
            SeasonEpisode = "S"+season+"E"+episode;
            ServerPath = serverPath;
            Season = season;
            Episode = episode;
            fullName = name + " " + SeasonEpisode;
            fullNameToShow = name + " "+ fullNameShow;
        }

    }
}
