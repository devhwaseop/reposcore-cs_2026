using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;

namespace RepoScore.Services
{
    public class GitHubService
    {
        private readonly Octokit.GraphQL.Connection _connection;
        private readonly string _owner;
        private readonly string _repo;

        private static readonly string[] s_claimKeywords =
            ["제가 하겠습니다", "진행하겠습니다", "할게요", "I'll take this"];

        public GitHubService(string owner, string repo, string token)
        {
            _owner = owner;
            _repo = repo;

            _connection = new Octokit.GraphQL.Connection(
                new Octokit.GraphQL.ProductHeaderValue("reposcore-cs"),
                token
            );
        }

        // PR 개수
        public async Task<int> GetPullRequestCountAsync(string authorLogin)
        {
            var query =
                new Query()
                .Search(
                    query: $"repo:{_owner}/{_repo} is:pr author:{authorLogin}",
                    type: SearchType.Issue,
                    first: 1)
                .Select(x => x.IssueCount);

            return await _connection.Run(query);
        }

        // Issue 개수
        public async Task<int> GetIssueCountAsync(string authorLogin)
        {
            var query =
                new Query()
                .Search(
                    query: $"repo:{_owner}/{_repo} is:issue author:{authorLogin}",
                    type: SearchType.Issue,
                    first: 1)
                .Select(x => x.IssueCount);

            return await _connection.Run(query);
        }

        // PR 댓글
        public async Task<List<string>> GetPullRequestCommentsAsync(int prNumber)
        {
            var query =
                new Query()
                .Repository(_owner, _repo)
                .PullRequest(prNumber)
                .Comments(first: 50)
                .Nodes
                .Select(c => c.Body);

            var result = await _connection.Run(query);

            return new List<string>(result);
        }

        // 최근 이슈 선점 현황 조회
        public async Task ShowRecentClaimsAsync()
        {
            var query = new Query()
                .Repository(_owner, _repo)
                .Issues(first: 20,
                        states: new[] { IssueState.Open },
                        orderBy: new IssueOrder
                        {
                            Field = IssueOrderField.CreatedAt,
                            Direction = OrderDirection.Desc
                        })
                .Nodes
                .Select(issue => new
                {
                    issue.Url,
                    Comments = issue.Comments(first: 10).Nodes
                        .Select(c => new
                        {
                            c.Body,
                            c.CreatedAt,
                            Login = c.Author.Login
                        }).ToList()
                });

            var issues = await _connection.Run(query);
            var now = DateTimeOffset.UtcNow;

            Console.WriteLine("📌 최근 이슈 선점 현황\n");

            foreach (var issue in issues)
            {
                foreach (var comment in issue.Comments)
                {
                    if ((now - comment.CreatedAt).TotalHours > 48)
                        continue;

                    if (s_claimKeywords.Any(k =>
                        comment.Body?.Contains(k, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        Console.WriteLine($"👤 {comment.Login}");
                        Console.WriteLine($" - {issue.Url}");
                        Console.WriteLine();
                        break;
                    }
                }
            }
        }
    }
}
