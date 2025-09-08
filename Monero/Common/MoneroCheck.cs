namespace Monero.Common
{
    public class MoneroCheck
    {
        protected bool? isGood;
    
        public MoneroCheck(bool? isGood = null)
        {
            this.isGood = isGood;
        }

        public virtual bool? IsGood() { return isGood; }

        public virtual MoneroCheck SetIsGood(bool? good)
        {
            isGood = good;
            return this;
        }
    }
}