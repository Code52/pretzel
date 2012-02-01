using Pretzel.Logic.Templating.Liquid;

namespace Pretzel.Tests.Templating.Liquid
{
    public class LiquidEngineTests :SpecificationFor<LiquidEngine>
    {
        public override LiquidEngine Given()
        {
            return new LiquidEngine();
        }

        public override void When()
        {
            // no - op
        }
    }
}
