using System;
using UnityEngine;
using Xunit;

namespace ServerDevcommands.Tests;

public class ParseTests
{
  [Fact]
  public void IntRange_ParsesNegativeRangeValues()
  {
    var range = Parse.IntRange("-1--9");

    Assert.Equal(-1, range.Min);
    Assert.Equal(-9, range.Max);
  }

  [Fact]
  public void FloatRange_ParsesNegativeRangeValues()
  {
    var range = Parse.FloatRange("-1.5--9.5");

    Assert.Equal(-1.5f, range.Min);
    Assert.Equal(-9.5f, range.Max);
  }

  [Fact]
  public void LongRange_ParsesNegativeRangeValues()
  {
    var range = Parse.LongRange("-100--50");

    Assert.Equal(-100L, range.Min);
    Assert.Equal(-50L, range.Max);
  }

  [Fact]
  public void Boolean_AndLogic_RecognizeNegativeAndTruthyStrings()
  {
    Assert.True(Parse.Boolean("true"));
    Assert.False(Parse.Boolean("false"));
    Assert.Null(Parse.BoolNull("maybe"));
    Assert.Equal("yes", Parse.Logic("true?yes:no"));
    Assert.Equal("no", Parse.Logic("false?yes:no"));
  }

  [Fact]
  public void VectorHelpers_ParseSignedValues()
  {
    var vector = Parse.VectorXZY("-1,2,-3");
    Assert.Equal(-1f, vector.x);
    Assert.Equal(-3f, vector.y);
    Assert.Equal(2f, vector.z);

    var range = Parse.VectorXZYRange("-1,-2,-3", new Vector3(0, 0, 0));
    Assert.Equal(-1f, range.Min.x);
    Assert.Equal(-3f, range.Min.y);
    Assert.Equal(-2f, range.Min.z);
  }

  [Fact]
  public void ScaleRange_HandlesSingleAndNegativeValues()
  {
    var single = Parse.ScaleRange("-2");
    Assert.Equal(-2f, single.Min.x);
    Assert.Equal(-2f, single.Min.y);
    Assert.Equal(-2f, single.Min.z);

    var range = Parse.ScaleRange("-1--2");
    Assert.Equal(-1f, range.Min.x);
    Assert.Equal(-2f, range.Max.x);
  }
}
