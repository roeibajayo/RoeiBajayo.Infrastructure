using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Utils.IEnumerable;

public class Ranges
{
    public static IReadOnlyList<Range> Group(IEnumerable<int> list)
    {
        ArgumentNullException.ThrowIfNull(list);

        var result = new List<Range>();
        var first = true;
        Range? current = null;

        //copy the list
        var sorted = list.ToArray();
        // faster sort
        Array.Sort(sorted);

        foreach (var item in sorted)
        {
            if (first)
            {
                first = false;
                current = new Range(item, item);
                continue;
            }

            if (current!.Value.End.Value + 1 == item)
            {
                current = new Range(current.Value.Start.Value, item);
            }
            else
            {
                result.Add(current.Value);
                current = new Range(item, item);
            }
        }

        if (current != null)
            result.Add(current.Value);

        return result;
    }
}
