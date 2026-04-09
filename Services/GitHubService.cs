using System;
using System.Linq;
using System.Threading.Tasks;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;

namespace RepoScore.Services
{
    public class GitHubService
    {
        private readonly Connection _connection;
        private readonly string _owner;
        private readonly string _repo;

        public GitHubService(string owner, string repo, string token)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));

            _connection = new Connection(
                new ProductHeaderValue("reposcore-cs"),
                token
            );
        }

        /// <summary>
        /// 이슈 + PR + 커밋을 단일 API 호출로 조회
        /// 필요한 데이터만 선택적으로 가져옴
        /// </summary>
        public async Task<object> GetRepositorySummaryAsync(DateTimeOffset? since = null)
        {
            try
            {
                var query =
                    from repo in _connection.Repository(_owner, _repo)
                    select new
                    {
                        // 이슈 (서버 필터링 + 필요한 필드만)
                        Issues = repo.Issues(
                                first: 20,
                                states: new[] { IssueState.Open, IssueState.Closed },
                                filterBy: since.HasValue
                                    ? new IssueFilters { Since = since.Value }
                                    : null
                            )
                            .Nodes
                            .Select(i => new
                            {
                                i.Number,
                                i.Title,
                                i.State,
                                i.CreatedAt,

                                // 댓글 일부만
                                Comments = i.Comments(first: 3)
                                    .Nodes
                                    .Select(c => new
                                    {
                                        c.Author.Login,
                                        c.Body
                                    })
                            }),

                        // PR (필요한 필드만)
                        PullRequests = repo.PullRequests(
                                first: 20,
                                states: new[] { PullRequestState.Open, PullRequestState.Closed }
                            )
                            .Nodes
                            .Select(pr => new
                            {
                                pr.Number,
                                pr.Title,
                                pr.State,
                                pr.CreatedAt,
                                Author = pr.Author.Login
                            }),

                        // 커밋 (브랜치 기준 + 일부만)
                        Commits = repo.Ref("refs/heads/main")
                            .Target
                            .Cast<Commit>()
                            .History(first: 20)
                            .Select(c => new
                            {
                                c.Message,
                                c.CommittedDate,
                                Author = c.Author.Name
                            }),

                        // Count만 필요한 경우 (핵심 최적화)
                        IssueCount = repo.Issues(null, null, null).TotalCount,
                        PullRequestCount = repo.PullRequests(null, null, null).TotalCount
                    };

                return await query.FirstAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"GraphQL 조회 실패: {ex.Message}", ex);
            }
        }
    }
}
