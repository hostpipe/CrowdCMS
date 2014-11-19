using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IEventTypesRepository
    {
        bool DeleteEventType(int eventTypeID);
        IQueryable<tbl_EventTypes> GetAll();
        tbl_EventTypes GetByID(int eventTypeID);
        tbl_EventTypes GetByName(string name);
        tbl_EventTypes SaveEventType(string title, string desc, int eventTypeID);
        tbl_EventTypes UpdatePath(string path, int eventTypeID);
    }

    public class EventTypesRepository : Repository<tbl_EventTypes>, IEventTypesRepository
    {
        public EventTypesRepository(IDALContext context) : base(context) { }

        public bool DeleteEventType(int eventTypeID)
        {
            var eventType = this.DbSet.FirstOrDefault(et => et.EventTypeID == eventTypeID);
            if (eventType == null)
                return false;
            if (eventType.tbl_Products.Any())
                return false;
            this.Delete(eventType);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_EventTypes> GetAll()
        {
            return this.All();
        }

        public tbl_EventTypes GetByID(int eventTypeID)
        {
            return this.DbSet.FirstOrDefault(et => et.EventTypeID == eventTypeID);
        }

        public tbl_EventTypes GetByName(string name)
        {
            return this.DbSet.FirstOrDefault(et => et.ET_Title.Equals(name));
        }

        public tbl_EventTypes SaveEventType(string title, string desc, int eventTypeID)
        {
            tbl_EventTypes eventType = this.DbSet.FirstOrDefault(et => et.EventTypeID == eventTypeID);
            if (eventType == null)
            {
                eventType = new tbl_EventTypes();
                this.Create(eventType);
            }

            eventType.ET_Title = title.Length > 100 ? title.Substring(0, 100) : title;
            eventType.ET_Description = desc;

            this.Context.SaveChanges();
            return eventType;
        }

        public tbl_EventTypes UpdatePath(string path, int eventTypeID)
        {
            tbl_EventTypes eventType = this.DbSet.FirstOrDefault(et => et.EventTypeID == eventTypeID);
            if (eventType == null)
                return null;

            eventType.ET_ImagePath = path;

            this.Context.SaveChanges();
            return eventType;
        }
    }
}
