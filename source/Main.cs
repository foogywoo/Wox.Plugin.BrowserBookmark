using System;
using System.Collections.Generic;
using System.Linq;

namespace Wox.Plugin.BrowserBookmark
{
    public class Main : IPlugin
    {
        private PluginInitContext context;

        // TODO: periodically refresh the Cache?
        private List<Bookmark> cachedBookmarks = new List<Bookmark>(); 

        public void Init(PluginInitContext context)
        {
            this.context = context;

            // Cache all bookmarks
            var chromeBookmarks = new ChromeBookmarks();
            var mozBookmarks = new FirefoxBookmarks();

            //TODO: Let the user select which browser's bookmarks are displayed
            // Add Firefox bookmarks
            cachedBookmarks.AddRange(mozBookmarks.GetBookmarks());
            // Add Chrome bookmarks
            cachedBookmarks.AddRange(chromeBookmarks.GetBookmarks());

            cachedBookmarks = cachedBookmarks.Distinct().ToList();
        }

        public List<Result> Query(Query query)
        {
            string param = query.GetAllRemainingParameter().TrimStart();

            // Should top results be returned? (true if no search parameters have been passed)
            var topResults = string.IsNullOrEmpty(param);
            
            var returnList = cachedBookmarks;

            if (!topResults)
            {
                // Since we mixed chrome and firefox bookmarks, we should order them again
                //var fuzzyMatcher = FuzzyMatcher.Create(param);
                returnList = cachedBookmarks.Where(o => MatchProgram(o, param)).ToList();
                returnList = returnList.OrderByDescending(o => o.Score).ToList();
            }
            
            return returnList.Select(c => new Result()
            {
                Title = c.Name,
                SubTitle = "Bookmark: " + c.Url,
                IcoPath = @"Images\bookmark.png",
                Score = 5,
                Action = (e) =>
                {
                    context.API.HideApp();
                    System.Diagnostics.Process.Start(c.Url);
                    return true;
                }
            }).ToList();
        }

        public static int CalcLevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (string.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }

        //will return the number of fuzzy matched words
        public static int FuzzyWordsMatchingCount(string a, string b)
        {
            int matchedWordsCount = 0;
            foreach (string wordi in a.Split(' '))
            {
                foreach (string wordj in b.Split(' '))
                {
                    int minLength = Math.Min(wordi.Length, wordj.Length);
                    int distance = CalcLevenshteinDistance(wordi.ToLower().Substring(0, minLength), wordj.ToLower().Substring(0, minLength));
                    if (distance < Math.Max(minLength / 2, 1))
                        matchedWordsCount++;
                }
            }

            return matchedWordsCount;
        }

        private bool MatchProgram(Bookmark bookmark, string param)
        {
            bookmark.Score = FuzzyWordsMatchingCount(bookmark.Name, param);

            return (bookmark.Score > 0);
        }
    }
}
