using Cocona;
using RepoScore.Data;
using RepoScore.Services;

var app = CoconaApp.Create();

app.AddCommand(async (
    [Argument] string repo,
    [Option('t', Description = "GitHub Personal Access Token")] string? token = null,
    [Option("show-claims", Description = "선점 현황 조회 (issue|user, 기본값: issue)")] string? showClaims = null
) =>
{
if (showClaims != null)
{
    if (string.IsNullOrEmpty(token))
        token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

    if (string.IsNullOrEmpty(token))
    {
        Console.WriteLine("GitHub 토큰이 필요합니다.");
        return;
    }

    var parts = repo.Split('/');
    var service = new GitHubService(parts[0], parts[1], token);
    var mode = string.IsNullOrEmpty(showClaims) ? "issue" : showClaims;
    await service.ShowRecentClaimsAsync(mode);
    return;
}

    Console.WriteLine($"저장소: {repo}");

    if (!string.IsNullOrEmpty(token))
    {
        Console.WriteLine($"토큰 인증 사용 중 (토큰: {token[..Math.Min(4, token.Length)]}***)");
    }
    else
    {
        Console.WriteLine("토큰 미입력 - 비인증 모드로 실행");
    }

    Console.WriteLine();
    Console.WriteLine("아이디, 문서이슈, 버그/기능이슈, 오타PR, 문서PR, 버그/기능PR, 총점");
    // 메서드 파라미터 순서: (기능/버그PR, 문서PR, 오타PR, 기능/버그이슈, 문서이슈)
    int user1Score = ScoreCalculator.CalculateFinalScore(1, 3, 1, 2, 1);
    Console.WriteLine($"user1, 1, 2, 1, 3, 1, {user1Score}");

    int user2Score = ScoreCalculator.CalculateFinalScore(2, 3, 5, 2, 1);
    Console.WriteLine($"user2, 1, 2, 5, 3, 2, {user2Score}");

    int user3Score = ScoreCalculator.CalculateFinalScore(5, 6, 5, 2, 3);
    Console.WriteLine($"user3, 3, 2, 5, 6, 5, {user3Score}");
});

app.Run();
