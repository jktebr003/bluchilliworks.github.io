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

    // Single post detail reducers
    [ReducerMethod(typeof(LoadPostDetailAction))]
    public static PostsState ReduceLoadPostDetailAction(PostsState state)
    {
        return state with
        {
            IsLoadingDetail = true,
            DetailErrorMessage = null
        };
    }

    [ReducerMethod]
    public static PostsState ReduceLoadPostDetailSuccessAction(PostsState state, LoadPostDetailSuccessAction action)
    {
        return state with
        {
            IsLoadingDetail = false,
            CurrentPost = action.Post,
            DetailErrorMessage = null
        };
    }

    [ReducerMethod]
    public static PostsState ReduceLoadPostDetailFailedAction(PostsState state, LoadPostDetailFailedAction action)
    {
        return state with
        {
            IsLoadingDetail = false,
            DetailErrorMessage = action.ErrorMessage,
            CurrentPost = null
        };
    }

    // Update post reducers
    [ReducerMethod(typeof(UpdatePostAction))]
    public static PostsState ReduceUpdatePostAction(PostsState state)
    {
        return state with
        {
            IsUpdating = true,
            UpdateErrorMessage = null
        };
    }

    [ReducerMethod]
    public static PostsState ReduceUpdatePostSuccessAction(PostsState state, UpdatePostSuccessAction action)
    {
        return state with
        {
            IsUpdating = false,
            CurrentPost = action.Post,
            UpdateErrorMessage = null
        };
    }

    [ReducerMethod]
    public static PostsState ReduceUpdatePostFailedAction(PostsState state, UpdatePostFailedAction action)
    {
        return state with
        {
            IsUpdating = false,
            UpdateErrorMessage = action.ErrorMessage
        };
    }
}
