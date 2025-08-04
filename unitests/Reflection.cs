using RoeiBajayo.Infrastructure.Reflection;
using System.Linq;
using Xunit;

namespace UnitTestProject;

public class Reflection
{
    public class SourceClass
    {
        public double Roei { get; set; }
        public string Bajayo { get; set; }
    }
    public class TargetClass
    {
        public double Roei { get; set; }
        public string Bajayo { get; set; }
    }

    [Fact]
    public void MapTo()
    {
        var source = new SourceClass
        {
            Roei = 1234.56,
            Bajayo = "thanks"
        };
        var result = source.MapTo<TargetClass>();
        Assert.Equal(typeof(TargetClass), result.GetType());
        Assert.Equal(source.Roei, result.Roei);
        Assert.Equal(source.Bajayo, result.Bajayo);

        // massive map

        int count = 10;
        SourceClass[] sources = new SourceClass[count];
        for (var i = 0; i < count; i++)
        {
            sources[i] = new SourceClass
            {
                Roei = 1234.56,
                Bajayo = "thanks"
            };
        }
        var results = sources.MapTo<TargetClass>().ToArray();
        for (var i = 0; i < count; i++)
        {
            source = sources[i];
            result = results[i];
            Assert.Equal(typeof(TargetClass), result.GetType());
            Assert.Equal(source.Roei, result.Roei);
            Assert.Equal(source.Bajayo, result.Bajayo);
        }
    }
}
