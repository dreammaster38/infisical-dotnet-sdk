using System.Collections.Concurrent;
using Xunit;

namespace Infisical.Sdk.Tests;

public class HarnessTests
{
  // xUnit runs test classes in parallel and every test starts its own fake server on a free port. Two servers that pick
  // the port at the same moment must not make a test fail.
  [Fact]
  public void Many_fake_servers_can_start_at_the_same_time()
  {
    var harnesses = new ConcurrentBag<Harness>();
    try
    {
      Parallel.For(0, 150, new ParallelOptions { MaxDegreeOfParallelism = 32 }, _ => harnesses.Add(new Harness()));
      Assert.Equal(150, harnesses.Count);
      Assert.Equal(150, harnesses.Select(h => h.BaseUrl).Distinct().Count());
    }
    finally
    {
      foreach (var harness in harnesses) harness.Dispose();
    }
  }
}
