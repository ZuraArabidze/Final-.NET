using Microsoft.EntityFrameworkCore;
using Reddit.Models;
using Reddit.Repositories;
using Reddit;
using Xunit.Abstractions;
namespace PageListTest_xUnit
{
    public class PagedListTests
    {
        private ApplicationDbContext CreateContext()
        {
            var databaseName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName).Options;

            var context = new ApplicationDbContext(options);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Posts.AddRange(
                new Post { Id = 1, Title = "Title 1", Content = "Content 1", Upvote = 5, Downvote = 1 },
                new Post { Id = 2, Title = "Title 2", Content = "Content 2", Upvote = 15, Downvote = 11 },
                new Post { Id = 3, Title = "Title 3", Content = "Content 3", Upvote = 25, Downvote = 21 },
                new Post { Id = 4, Title = "Title 4", Content = "Content 4", Upvote = 35, Downvote = 31 },
                new Post { Id = 5, Title = "Title 5", Content = "Content 5", Upvote = 45, Downvote = 41 },
                new Post { Id = 6, Title = "Title 6", Content = "Content 6", Upvote = 55, Downvote = 51 },
                new Post { Id = 7, Title = "Title 7", Content = "Content 7", Upvote = 65, Downvote = 61 },
                new Post { Id = 8, Title = "Title 8", Content = "Content 8", Upvote = 75, Downvote = 71 },
                new Post { Id = 9, Title = "Title 9", Content = "Content 9", Upvote = 85, Downvote = 81 },
                new Post { Id = 10, Title = "Title 10", Content = "Content 10", Upvote = 95, Downvote = 91 }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task CreateAsync_ReturnsCorrectPagination()
        {
            var context = CreateContext();
            var items = context.Posts.AsQueryable();
            var pageNumber = 1;
            var pagesize = 2;
            var result = await PagedList<Post>.CreateAsync(items, pageNumber, pagesize);
            Assert.Equal(pagesize,result.Items.Count);
            Assert.Equal(pagesize,result.PageSize);
        }

        [Fact]
        public async Task CreateAsync_ReturnsCorrectItems()
        {
            using var context = CreateContext();
            var items = context.Posts.AsQueryable();
            var pageNumber = 1;
            var pageSize = 5;
            var result = await PagedList<Post>.CreateAsync(items, pageNumber, pageSize);
            Assert.Equal(5, result.Items.Count);
        }

        [Fact]
        public async Task CreateAsync_HasNextPage_ReturnsCorrectValue()
        {
            using var context = CreateContext();
            var items = context.Posts.AsQueryable();
            var firstPage = await PagedList<Post>.CreateAsync(items, 1, 5);
            var lastPage = await PagedList<Post>.CreateAsync(items, 2, 5);
            Assert.True(firstPage.HasNextPage);
            Assert.False(lastPage.HasNextPage);
        }

        [Fact]
        public async Task CreateAsync_HasPreviousPage_ReturnsCorrectValue()
        {
            using var context = CreateContext();
            var items = context.Posts.AsQueryable();
            var firstPage = await PagedList<Post>.CreateAsync(items, 1, 2);
            var middlePage = await PagedList<Post>.CreateAsync(items, 2, 2);
            Assert.False(firstPage.HasPreviousPage);
            Assert.True(middlePage.HasPreviousPage);
        }

        [Fact]
        public async Task CreateAsync_EmptyDatabase_ReturnsEmptyList()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            using var context = new ApplicationDbContext(options);
            var items = context.Posts.AsQueryable();
            var result = await PagedList<Post>.CreateAsync(items, 1, 5);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
        }

        [Fact]
        public async Task CreateAsync_IsLargerPagesizeThanItems()
        {
            using var context = CreateContext();
            var items = context.Posts.AsQueryable();
            PagedList<Post> pagedList = await PagedList<Post>.CreateAsync(items, 1, 15);
            Assert.Equal(10, pagedList.Items.Count);
            Assert.True(!pagedList.HasNextPage);
        }

        [Fact]
        public async Task CreateAsync_IsWhenTotalCountIsLargerThanPagesize()
        {
            using var context = CreateContext();
            var items = context.Posts.AsQueryable();
            PagedList<Post> pagedList = await PagedList<Post>.CreateAsync(items, 1, 3);
            Assert.Equal(10, pagedList.TotalCount);
            Assert.True(pagedList.HasNextPage);
        }

        [Fact]
        public async Task CreateAsync_ThrowsOnInvalidPageNumber()
        {
            using var context = CreateContext();
            var items = context.Posts.AsQueryable();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PagedList<Post>.CreateAsync(items, 0, 5));
        }

        [Fact]
        public async Task CreateAsync_ThrowsOnInvalidPageSize()
        {
            using var context = CreateContext();
            var items = context.Posts.AsQueryable();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PagedList<Post>.CreateAsync(items, 1, 0));
        }

        [Fact]
        public async Task CreateAsync_IsPageNumberSetToNegativeThrowing()
        {
            using var context = CreateContext();
            var items = context.Posts.AsQueryable();
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await PagedList<Post>.CreateAsync(items, -1, 5);
            });
        }
        [Fact]
        public async Task CreateAsync_IsPagesizeSetToNegativeThrowing()
        {
            using var context = CreateContext();
            var items = context.Posts.AsQueryable();
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await PagedList<Post>.CreateAsync(items, 1, -3);
            });
        }
    }
}
