using System;
using System.Linq;
using Xunit;

namespace ServerDevcommands.Tests;

public class SearchOutputTests
{
  [Fact]
  public void TakeLog_LogAfterRequiredIsFlag()
  {
    var values = new[] { "wolf", "log" };
    Assert.True(SearchOutput.TakeLog(ref values, 1));
    Assert.Equal(["wolf"], values);
  }

  [Fact]
  public void TakeLog_LogAsOnlyValueIsSearchText()
  {
    var values = new[] { "log" };
    Assert.False(SearchOutput.TakeLog(ref values, 1));
    Assert.Equal(["log"], values);
  }

  [Fact]
  public void TakeLog_IsCaseInsensitive()
  {
    var values = new[] { "log", "LOG" };
    Assert.True(SearchOutput.TakeLog(ref values, 1));
    Assert.Equal(["log"], values);
  }

  [Fact]
  public void TakeLog_ExtraValueThrows()
  {
    var values = new[] { "wolf", "10" };
    Assert.Throws<InvalidOperationException>(() => SearchOutput.TakeLog(ref values, 1));
  }

  [Fact]
  public void TakeLog_UnlimitedKeepsAllTerms()
  {
    var values = new[] { "a,1", "b,2", "log" };
    Assert.True(SearchOutput.TakeLog(ref values, 1, true));
    Assert.Equal(["a,1", "b,2"], values);
  }

  [Fact]
  public void Wrap_BreaksRowsAtWidth()
  {
    var (rows, hidden) = SearchOutput.Wrap(["aaaa", "bbbb", "cccc"], 10);
    Assert.Equal(["aaaa, bbbb", "cccc"], rows);
    Assert.Equal(0, hidden);
  }

  [Fact]
  public void Wrap_LongValueGetsOwnRow()
  {
    var (rows, _) = SearchOutput.Wrap(["aaaaaaaaaaaa", "b"], 5);
    Assert.Equal(["aaaaaaaaaaaa", "b"], rows);
  }

  [Fact]
  public void Wrap_CapsRowsAndCountsHidden()
  {
    var values = Enumerable.Range(0, 10).Select(i => "v" + i).ToList();
    var (rows, hidden) = SearchOutput.Wrap(values, 2, 4);
    Assert.Equal(["v0", "v1", "v2", "v3"], rows);
    Assert.Equal(6, hidden);
  }

  [Fact]
  public void Wrap_NoTrailingComma()
  {
    var (rows, _) = SearchOutput.Wrap(["a", "b"], 100);
    Assert.Equal(["a, b"], rows);
  }
}
