using homeCookAPI.Models;
using Microsoft.EntityFrameworkCore;

public class CommentRepository : ICommentRepository
{
    private readonly ApplicationDbContext _context;

    public CommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Comment>> GetAllAsync()
    {
        return await _context.Comments.Include(c => c.User).ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetRepliesAsync(int commentId)
    {
        return await _context.Comments
            .Where(c => c.ParentCommentId == commentId)
            .Include(c => c.User)
            .ToListAsync();
    }

    public async Task<Comment> GetByIdAsync(int commentId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CommentId == commentId);
    }

    public async Task<bool> ExistsAsync(int commentId)
    {
        return await _context.Comments.AnyAsync(c => c.CommentId == commentId);
    }

    public async Task AddAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Comment comment)
    {
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }
}
