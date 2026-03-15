using Fluxor;
using MediatR;

using MudBlazorWeb.Features.Posts.Application;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Posts.UI;

public class PostsEffects
{
    private readonly IMediator _mediator;

    public PostsEffects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [EffectMethod]
    public async Task HandleLoadPosts(LoadPostsAction action, IDispatcher dispatcher)
    {
        try
        {
            await Task.Yield();

            var result = await _mediator.Send(new GetPostsQuery.Query(action.PageSize, action.PageNumber));

            if (result.Success && result.Value != null)
            {
                var posts = result.Value.Select(MapToPostResponse).ToList();

                dispatcher.Dispatch(new LoadPostsSuccessAction(
                    posts,
                    result.TotalItems,
                    result.TotalPages,
                    result.PageNumber ?? action.PageNumber,
                    result.PageSize ?? action.PageSize
                ));
            }
            else
            {
                dispatcher.Dispatch(new LoadPostsFailedAction(result.Message ?? "Failed to load posts"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadPostsFailedAction($"Error loading posts: {ex.Message}"));
        }
    }

    [EffectMethod]
    public Task HandleChangePage(ChangePageAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LoadPostsAction(action.PageNumber));
        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleLoadPostDetail(LoadPostDetailAction action, IDispatcher dispatcher)
    {
        try
        {
            await Task.Yield();

            if (!Guid.TryParse(action.PostId, out var postId))
            {
                dispatcher.Dispatch(new LoadPostDetailFailedAction("Invalid post ID."));
                return;
            }

            var result = await _mediator.Send(new GetPostQuery.Query(postId));

            if (result.Success && result.Value != null)
            {
                dispatcher.Dispatch(new LoadPostDetailSuccessAction(MapToPostResponse(result.Value)));
            }
            else
            {
                dispatcher.Dispatch(new LoadPostDetailFailedAction(result.Message ?? "Failed to load post details"));
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
            await Task.Yield();

            var result = await _mediator.Send(new UpdatePostCommand.Command(action.Request));

            if (result.Success)
            {
                dispatcher.Dispatch(new LoadPostDetailAction(action.Request.Id));
            }
            else
            {
                dispatcher.Dispatch(new UpdatePostFailedAction(result.Message ?? "Failed to update post"));
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
            await Task.Yield();

            var result = await _mediator.Send(new CreatePostCommand.Command(action.Request));

            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                dispatcher.Dispatch(new CreatePostSuccessAction(result.Value));
            }
            else
            {
                dispatcher.Dispatch(new CreatePostFailedAction(result.Message ?? "Failed to create post"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CreatePostFailedAction($"Error creating post: {ex.Message}"));
        }
    }

    private static PostResponse MapToPostResponse(GetPostsQuery.PostDto dto)
    {
        return new PostResponse
        {
            ID = dto.Id.ToString(),
            Title = dto.Title,
            Heading = dto.Heading,
            Description = dto.Description,
            Content = dto.Content,
            Author = dto.Author,
            UserId = dto.UserId,
            Category = dto.Category,
            TotalViews = dto.TotalViews,
            TotalLikes = dto.TotalLikes,
            PostedOn = dto.PostedOn.ToString("O"),
            CreatedOn = dto.CreatedOn.ToString("O"),
            CreatedBy = dto.CreatedBy,
            ModifiedOn = dto.ModifiedOn?.ToString("O"),
            ModifiedBy = dto.ModifiedBy,
            DeletedOn = dto.DeletedOn?.ToString("O"),
            DeletedBy = dto.DeletedBy,
            IsDeleted = dto.IsDeleted
        };
    }

    private static PostResponse MapToPostResponse(GetPostQuery.PostDto dto)
    {
        return new PostResponse
        {
            ID = dto.Id.ToString(),
            Title = dto.Title,
            Heading = dto.Heading,
            Description = dto.Description,
            Content = dto.Content,
            Author = dto.Author,
            UserId = dto.UserId,
            Category = dto.Category,
            TotalViews = dto.TotalViews,
            TotalLikes = dto.TotalLikes,
            PostedOn = dto.PostedOn.ToString("O"),
            CreatedOn = dto.CreatedOn.ToString("O"),
            CreatedBy = dto.CreatedBy,
            ModifiedOn = dto.ModifiedOn?.ToString("O"),
            ModifiedBy = dto.ModifiedBy,
            DeletedOn = dto.DeletedOn?.ToString("O"),
            DeletedBy = dto.DeletedBy,
            IsDeleted = dto.IsDeleted
        };
    }
}

