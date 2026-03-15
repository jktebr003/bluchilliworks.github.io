using MudBlazorWeb.Features.Posts.UI;
using MudBlazorWeb.Shared.Models;

using Xunit;

namespace MudBlazorWeb.Features.Posts.Tests;

public class PostReducerTests
{
    [Fact]
    public void ReduceLoadPostsAction_ShouldSetLoadingTrue_AndClearError()
    {
        var state = new PostsState
        {
            IsLoading = false,
            ErrorMessage = "old",
            Posts = [CreatePostResponse("1", "Old")]
        };

        var result = PostsReducers.ReduceLoadPostsAction(state);

        Assert.NotSame(state, result);
        Assert.True(result.IsLoading);
        Assert.Null(result.ErrorMessage);
        Assert.Single(result.Posts);
    }

    [Fact]
    public void ReduceLoadPostsSuccessAction_ShouldPopulatePagedData()
    {
        var posts = new List<PostResponse>
        {
            CreatePostResponse("1", "One"),
            CreatePostResponse("2", "Two")
        };

        var state = new PostsState { IsLoading = true, ErrorMessage = "old" };
        var action = new LoadPostsSuccessAction(posts, 20, 10, 2, 2);

        var result = PostsReducers.ReduceLoadPostsSuccessAction(state, action);

        Assert.False(result.IsLoading);
        Assert.Null(result.ErrorMessage);
        Assert.Same(posts, result.Posts);
        Assert.Equal(20, result.TotalItems);
        Assert.Equal(10, result.TotalPages);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public void ReduceLoadPostsFailedAction_ShouldSetError_AndClearPosts()
    {
        var state = new PostsState
        {
            IsLoading = true,
            Posts = [CreatePostResponse("1", "One")]
        };

        var result = PostsReducers.ReduceLoadPostsFailedAction(state, new LoadPostsFailedAction("failure"));

        Assert.False(result.IsLoading);
        Assert.Equal("failure", result.ErrorMessage);
        Assert.Empty(result.Posts);
    }

    [Fact]
    public void ReduceLoadPostDetailAction_ShouldResetDetailState()
    {
        var state = new PostsState
        {
            IsLoadingDetail = false,
            IsUpdating = true,
            DetailErrorMessage = "old",
            CurrentPost = CreatePostResponse("1", "One")
        };

        var result = PostsReducers.ReduceLoadPostDetailAction(state);

        Assert.True(result.IsLoadingDetail);
        Assert.False(result.IsUpdating);
        Assert.Null(result.DetailErrorMessage);
        Assert.Null(result.CurrentPost);
    }

    [Fact]
    public void ReduceLoadPostDetailSuccessAction_ShouldSetCurrentPost()
    {
        var post = CreatePostResponse("2", "Detail");
        var state = new PostsState { IsLoadingDetail = true, DetailErrorMessage = "old" };

        var result = PostsReducers.ReduceLoadPostDetailSuccessAction(state, new LoadPostDetailSuccessAction(post));

        Assert.False(result.IsLoadingDetail);
        Assert.Null(result.DetailErrorMessage);
        Assert.Same(post, result.CurrentPost);
    }

    [Fact]
    public void ReduceLoadPostDetailFailedAction_ShouldSetDetailError_AndClearCurrentPost()
    {
        var state = new PostsState
        {
            IsLoadingDetail = true,
            CurrentPost = CreatePostResponse("2", "Detail")
        };

        var result = PostsReducers.ReduceLoadPostDetailFailedAction(state, new LoadPostDetailFailedAction("not found"));

        Assert.False(result.IsLoadingDetail);
        Assert.Equal("not found", result.DetailErrorMessage);
        Assert.Null(result.CurrentPost);
    }

    [Fact]
    public void ReduceUpdatePostActions_ShouldTrackUpdateLifecycle()
    {
        var state = new PostsState
        {
            IsUpdating = false,
            UpdateErrorMessage = "old",
            CurrentPost = CreatePostResponse("1", "One")
        };

        var loading = PostsReducers.ReduceUpdatePostAction(state);
        Assert.True(loading.IsUpdating);
        Assert.Null(loading.UpdateErrorMessage);

        var updatedPost = CreatePostResponse("1", "Updated");
        var success = PostsReducers.ReduceUpdatePostSuccessAction(loading, new UpdatePostSuccessAction(updatedPost));
        Assert.False(success.IsUpdating);
        Assert.Null(success.UpdateErrorMessage);
        Assert.Same(updatedPost, success.CurrentPost);

        var failed = PostsReducers.ReduceUpdatePostFailedAction(loading, new UpdatePostFailedAction("update failed"));
        Assert.False(failed.IsUpdating);
        Assert.Equal("update failed", failed.UpdateErrorMessage);
    }

    [Fact]
    public void ReduceCreatePostActions_ShouldTrackCreateLifecycle()
    {
        var state = new PostsState
        {
            IsCreating = false,
            CreateErrorMessage = "old"
        };

        var loading = PostsReducers.ReduceCreatePostAction(state);
        Assert.True(loading.IsCreating);
        Assert.Null(loading.CreateErrorMessage);

        var success = PostsReducers.ReduceCreatePostSuccessAction(loading, new CreatePostSuccessAction("id-1"));
        Assert.False(success.IsCreating);
        Assert.Null(success.CreateErrorMessage);

        var failed = PostsReducers.ReduceCreatePostFailedAction(loading, new CreatePostFailedAction("create failed"));
        Assert.False(failed.IsCreating);
        Assert.Equal("create failed", failed.CreateErrorMessage);
    }

    private static PostResponse CreatePostResponse(string id, string title)
    {
        return new PostResponse
        {
            ID = id,
            Title = title,
            Heading = "heading",
            Content = "content",
            Author = "author",
            Category = "category"
        };
    }
}
