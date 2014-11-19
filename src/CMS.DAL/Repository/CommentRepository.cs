using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{

    public interface ICommentRepository
    {
        tbl_Comments AuthoriseComment(int commentID);
        bool DeleteComment(int commentID);
        IQueryable<tbl_Comments> GetNewsComments(int sitemapID);
        tbl_Comments SaveComment(string name, string email, string website, string message, int sitemapID, int commentID);
    }

    public class CommentRepository : Repository<tbl_Comments>, ICommentRepository
    {
        public CommentRepository(IDALContext context) : base(context) { }

        public tbl_Comments AuthoriseComment(int commentID)
        {
            var comment = this.DbSet.FirstOrDefault(c => c.CommentID == commentID);
            if (comment == null)
                return null;

            comment.CO_Authorised = true;
            this.Context.SaveChanges();
            return comment;
        }

        public bool DeleteComment(int commentID)
        {
            var comment = this.DbSet.FirstOrDefault(c => c.CommentID == commentID);
            if (comment == null)
                return false;

            this.Delete(comment);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Comments> GetNewsComments(int sitemapID)
        {
            return this.DbSet.Where(c => c.CO_SiteMapID == sitemapID);
        }

        public tbl_Comments SaveComment(string name, string email, string website, string message, int sitemapID, int commentID)
        {
            var comment = this.DbSet.FirstOrDefault(c => c.CommentID == commentID);
            if (comment == null)
            {
                comment = new tbl_Comments()
                {
                    CO_Authorised = false,
                    CO_Date = DateTime.UtcNow,
                    CO_SiteMapID = sitemapID
                };

                this.Create(comment);
            }

            comment.CO_Author = name;
            comment.CO_Comment = message ?? String.Empty;
            comment.CO_Email = email;
            comment.CO_URL = website ?? String.Empty;

            this.Context.SaveChanges();
            return comment;
        }
    }

}
