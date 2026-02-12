using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Service.ID
{
    public class IDService: IIDService
    {
        private readonly TicketsContext _context;
        public IDService(TicketsContext context)
        {
            _context = context;
        }
        public async Task<string> GetNextProgrammeID()
        {
            var pid=await _context.Database.SqlQuery<string>($"Select dbo.funGetProgrammeID() AS Value").FirstOrDefaultAsync();
            return pid;
        }
        public async Task<string> GetNextSessionID(string pid)
        {
            var sid=await _context.Database.SqlQuery<string>($"Select dbo.funGetSessionID({pid}) AS Value").FirstOrDefaultAsync();
            return sid;
        }
        public async Task<string> GetNextTicketsAreaID(string sid)
        {
            var nextId = _context.Database.SqlQueryRaw<string>($"SELECT [dbo].[funGetTicketsAreaID](@p0)", sid).AsEnumerable().FirstOrDefault();
            return nextId;
        }

        public async Task<string> GetNextFAQTypeID()
        {
            var faqtypeId = _context.Database.SqlQueryRaw<string>($"SELECT dbo.funGetFAQTypeID() as value").AsEnumerable().FirstOrDefault();
            return faqtypeId;
        }
    }
}