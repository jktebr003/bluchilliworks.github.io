using Fluxor;
using Shared.Models;
using Web.Shared;

namespace Web.Features.Posts;

public class PostsEffects
{
    private readonly WebApiClient _apiClient;

    public PostsEffects(WebApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [EffectMethod]
    public async Task HandleLoadPosts(LoadPostsAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _apiClient.Get<PagedApiResult<List<PostResponse>>>(
                new WebApiClientInfo<object> 
                { 
                    Method = $"/posts", 
                    Request = $"?pageSize={action.PageSize}&pageNumber={action.PageNumber}"
                }
            );

            if (result?.Success == true && result.Value != null)
            {
                dispatcher.Dispatch(new LoadPostsSuccessAction(
                    result.Value,
                    result.TotalItems,
                    result.TotalPages,
                    action.PageNumber,
                    action.PageSize
                ));
            }
            else
            {
                dispatcher.Dispatch(new LoadPostsFailedAction(result?.Message ?? "Failed to load posts"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadPostsFailedAction($"Error loading posts: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleChangePage(ChangePageAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LoadPostsAction(action.PageNumber));
    }
}
