namespace Grillbot.Database
{
    public abstract class RepositoryBase
    {
        protected GrillBotContext Context { get; set; }

        protected RepositoryBase(GrillBotContext context)
        {
            Context = context;
        }
    }
}
