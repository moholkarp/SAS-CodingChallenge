using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstructionLine.CodingChallenge
{
    public class SearchEngine
    {
        private readonly List<Shirt> _shirts;
        private List<Shirt> lstShirts;
        private List<SizeCount> lstSizeCount = new List<SizeCount>();
        private List<ColorCount> lstColorCount = new List<ColorCount>();

        public SearchEngine(List<Shirt> shirts)
        {
            _shirts = shirts;
        }

        public SearchResults Search(SearchOptions options)
        {
            // TODO: search logic goes here.
            if (options == null || options.Colors == null || options.Sizes == null)
                throw new ArgumentNullException(nameof(options));

            var cbShirts = new ConcurrentBag<Shirt>();
            Parallel.ForEach(_shirts, _shirt =>
                {
                    if ((!options.Colors.Any() || options.Colors.Contains(_shirt.Color)) &&
                        (!options.Sizes.Any() || options.Sizes.Contains(_shirt.Size)))
                    {
                        cbShirts.Add(_shirt);
                    }
                }
            );
            lstShirts = cbShirts.ToList();

            Parallel.Invoke(
                () =>
                {
                    var cbSizeCount = new ConcurrentBag<SizeCount>();
                    Parallel.ForEach(Size.All, size =>
                        {
                            cbSizeCount.Add(new SizeCount
                            {
                                Size = size,
                                Count = lstShirts
                                .Count(shirt => shirt.Size.Id == size.Id
                                            && (!options.Sizes.Any()
                                                || options.Sizes.Select(s => s.Id).Contains(shirt.Size.Id))
                                            && (!options.Colors.Any()
                                                || options.Colors.Select(c => c.Id).Contains(shirt.Color.Id)))
                            });
                        }
                    );
                    lstSizeCount = cbSizeCount.ToList();
                },
                () =>
                {
                    var cbColorCount = new ConcurrentBag<ColorCount>();
                    Parallel.ForEach(Color.All, color =>
                        {
                            cbColorCount.Add(new ColorCount
                            {
                                Color = color,
                                Count = lstShirts
                                .Count(shirt => shirt.Color.Id == color.Id
                                            && (!options.Sizes.Any()
                                                || options.Sizes.Select(s => s.Id).Contains(shirt.Size.Id))
                                            && (!options.Colors.Any()
                                                || options.Colors.Select(c => c.Id).Contains(shirt.Color.Id)))
                            });
                        }
                    );
                    lstColorCount = cbColorCount.ToList();
                });

            return new SearchResults
            {
                Shirts = lstShirts,
                SizeCounts = lstSizeCount,
                ColorCounts = lstColorCount
            };
        }
    }
}