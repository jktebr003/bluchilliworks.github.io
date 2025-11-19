using Fluxor;
using Shared.Models;

namespace Web.Features.Posts;

public static class PostsReducers
{
    [ReducerMethod(typeof(LoadPostsAction))]
    public static PostsState ReduceLoadPostsAction(PostsState state)
    {
        return state with
        {
            IsLoading = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static PostsState ReduceLoadPostsSuccessAction(PostsState state, LoadPostsSuccessAction action)
    {
        return state with
        {
            IsLoading = false,
            Posts = action.Posts,
            TotalItems = action.TotalItems,
            TotalPages = action.TotalPages,
            CurrentPage = action.PageNumber,
            PageSize = action.PageSize,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static PostsState ReduceLoadPostsFailedAction(PostsState state, LoadPostsFailedAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage,
            Posts = new List<PostResponse>()
        };
    }

    [ReducerMethod]
    public static PostsState ReduceChangePageAction(PostsState state, ChangePageAction action)
    {
        return state with
        {
            CurrentPage = action.PageNumber
        };
    }
}
