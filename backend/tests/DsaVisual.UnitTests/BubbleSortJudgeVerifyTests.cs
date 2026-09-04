using DsaVisual.Application.Services;

namespace DsaVisual.UnitTests;

/// <summary>VERIFY tạm thời (xóa sau): chấm bubble sort input→output trên Jint server.</summary>
public class BubbleSortJudgeVerifyTests
{
    private static readonly CodelabTaskSpec BubbleSortTask = new(
        "verify-bubble-sort",
        "bubbleSort",
        [
            new CodelabTestCaseSpec("[[3,1,2]]", "[1,2,3]", false),
            new CodelabTestCaseSpec("[[5,4,3,2,1]]", "[1, 2, 3, 4, 5]", false), // expected chứa space → normalize phải bỏ qua
            new CodelabTestCaseSpec("[[1]]", "[1]", false),
            new CodelabTestCaseSpec("[[2,2,9,1]]", "[1,2,2,9]", true),
        ]);

    private const string CorrectCode = """
        function bubbleSort(a) {
          const n = a.length;
          for (let i = 0; i < n - 1; i++) {
            for (let j = 0; j < n - i - 1; j++) {
              if (a[j] > a[j + 1]) {
                const t = a[j]; a[j] = a[j + 1]; a[j + 1] = t;
              }
            }
          }
          return a;
        }
        """;

    [Fact]
    public void BubbleSort_CorrectCode_AllCasesPass()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge(CorrectCode, BubbleSortTask);

        Assert.False(result.CompileError);
        Assert.False(result.TimedOut);
        Assert.Equal(4, result.Cases.Count);
        Assert.All(result.Cases, c => Assert.True(c.Passed, $"case lỗi: {c.Error}"));
    }

    [Fact]
    public void BubbleSort_OldStarter_ReturnInput_MustFail()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge("""
            function bubbleSort(input) {
              // TODO: Viết mã nguồn ở đây
              return input;
            }
            """, BubbleSortTask);

        Assert.False(result.CompileError);
        var passedCount = result.Cases.Count(c => c.Passed);
        Assert.True(passedCount < result.Cases.Count, "code KHÔNG sắp xếp (return input như starter cũ) phải FAIL ít nhất 1 case");
    }

    [Fact]
    public void BubbleSort_WrongDirection_Desc_MustFail()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge(CorrectCode.Replace("a[j] > a[j + 1]", "a[j] < a[j + 1]"), BubbleSortTask);

        var passedCount = result.Cases.Count(c => c.Passed);
        Assert.True(passedCount < result.Cases.Count, "sắp xếp DESC thay vì ASC phải FAIL");
    }

    [Fact]
    public void BubbleSort_EmptyCode_MustBeRejected()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge("   \n\t ", BubbleSortTask);

        Assert.True(result.CompileError);
        Assert.Contains("trống", result.CompileErrorText);
        Assert.Empty(result.Cases);
    }

    [Fact]
    public void BubbleSort_InfiniteLoop_TimedOut()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge("function bubbleSort(a) { while (true) {} }", BubbleSortTask);

        Assert.True(result.TimedOut);
    }
}
