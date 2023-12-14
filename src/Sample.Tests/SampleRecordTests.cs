namespace Sample.Tests;

public class SampleRecordTests
{
    [Fact]
    public void SampleRecord_Value()
    {
        var sut = new SampleRecord("string");
        Assert.Equal("string", sut.Value);
    }
    
    [Fact]
    public void UpperRecord_Value()
    {
        var sut = new UpperRecord("string");
        Assert.Equal("STRING", sut.Value);
    }
    
    [Fact]
    public void ValidatedRecord_Throw()
    {
        Assert.Throws<ArgumentException>(() => ValidatedRecord.Parse("four"));
    }
    
    [Fact]
    public void ValidatedRecord_Ok()
    {
        ValidatedRecord.Parse("fourAndFive");
        Assert.True(ValidatedRecord.TryParse("fourAndFive", out _));
    }

}