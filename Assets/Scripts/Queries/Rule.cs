using Criteria;
using Remember;

namespace Assets.Scripts.Queries
{
    abstract class Rule
    {
        public abstract ICriterion[] Criteria { get; }

        public abstract IRememberer[] Rememberers { get; }

        public int NumCriteria => Criteria.Length;

        /// <summary>
        /// Evaluate criteria and return true if all pass.
        /// </summary>
        public virtual bool Evaluate(Query query)
        {
            foreach (var c in Criteria)
            {
                if (!c.Eval(query))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Actions to take in response to stimulus.
        /// </summary>
        public virtual void Response(Query query)
        {
        }

        /// <summary>
        /// Any state to store/manipulate as a result of this rule's successful passing.
        /// </summary>
        public virtual void Remember(Query query)
        {
            foreach (var rememberer in Rememberers)
            {
                rememberer.Apply(query);
            }
        }
    }
}
