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

    [EffectMethod]
    public async Task HandleLoadPostDetail(LoadPostDetailAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _apiClient.Get<ApiResult<PostResponse>>(
                new WebApiClientInfo<object> 
                { 
                    Method = $"/posts/{action.PostId}",
                    Request = string.Empty
                }
            );

            if (result?.Success == true && result.Value != null)
            {
                dispatcher.Dispatch(new LoadPostDetailSuccessAction(result.Value));
            }
            else
            {
                dispatcher.Dispatch(new LoadPostDetailFailedAction(result?.Message ?? "Failed to load post details"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadPostDetailFailedAction($"Error loading post details: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleUpdatePost(UpdatePostAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _apiClient.Put<UpdatePostRequest, ApiResult<string>>(
                new WebApiClientInfo<UpdatePostRequest>
                {
                    Method = "/posts",
                    Request = action.Request
                }
            );

            if (result?.Success == true)
            {
                // After successful update, reload the post details
                dispatcher.Dispatch(new LoadPostDetailAction(action.Request.Id));
            }
            else
            {
                dispatcher.Dispatch(new UpdatePostFailedAction(result?.Message ?? "Failed to update post"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new UpdatePostFailedAction($"Error updating post: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleCreatePost(CreatePostAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _apiClient.Post<CreatePostRequest, ApiResult<string>>(
                new WebApiClientInfo<CreatePostRequest>
                {
                    Method = "/posts",
                    Request = action.Request
                }
            );

            if (result?.Success == true && !string.IsNullOrEmpty(result.Value))
            {
                dispatcher.Dispatch(new CreatePostSuccessAction(result.Value));
            }
            else
            {
                dispatcher.Dispatch(new CreatePostFailedAction(result?.Message ?? "Failed to create post"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CreatePostFailedAction($"Error creating post: {ex.Message}"));
        }
    }
}
