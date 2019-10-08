using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Threading.Tasks;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Commands
{
    [Shared]
    [Export]
    [CommandArguments]
    public class HungryCommandArguments : BaseCommandArguments
    {
        protected override IEnumerable<Option> CreateOptions() => Array.Empty<Option>();
    }

    [Shared]
    [Export]
    [CommandInfo(
        Name = "hungry",
        Description = "use only when hungry",
        ArgumentsType = typeof(HungryCommandArguments),
        CommandType = typeof(HungryCommand)
        )]
    public sealed class HungryCommand : Command<HungryCommandArguments>
    {
        private readonly string recipe = @"===== Ingredients ======

== Dough====
2 ½ cups Plain Flour
½ teaspoon salt
1 teaspoon sugar
2 ¼ teaspoons dried yeast
1 cup warm water

== Topping====
½ cup warm water
2 tablespoons baking soda
coarse salt(sea salt, for example)
2 tablespoons unsalted butter, melted OR about a tablespoon of olive oil
If you want to make sweet pretzels rather than savoury/traditional pretzels, replace the coarse salt topping with a mixture of cocoa, cinnamon and a little icing sugar.


===== Process ======
1. Place all of the dough ingredients into a bowl, and beat till well-combined.Knead the dough, for about 5 minutes, till it's soft, smooth, and quite slack.

2. Flour the dough in a bowl, cover and allow it to rest for 30 minutes.

3. Preheat your oven to 240°C (fan forced, 260c fanless).

4. Prepare two baking sheets/trays by spraying them with oil spray, or lining them with baking paper.

5. Transfer the dough to a lightly greased work surface, and divide it into eight equal pieces (about 70g each). Allow the pieces to rest, uncovered, for 5 minutes.

6. While the dough is resting, combine the 1/2 cup warm water and the baking soda, and place it in a shallow bowl.Make sure the baking soda is thoroughly dissolved; if it isn't, it'll make your pretzels splotchy.

7. Roll each piece of dough into a long, thin rope(about 70cm long), and twist each rope into a pretzel.Dip each pretzel in the baking soda wash(this will give the pretzels a nice, golden-brown color), and place them on the baking sheets.Sprinkle lightly with salt(or sweet pretzel mix outlined above). Allow them to rest, uncovered, for 10 minutes.

8. Bake the pretzels for 7 to 8 minutes, or until they're golden brown, reversing the baking sheets halfway through.

9. Remove the pretzels from the oven, and brush them thoroughly with the melted butter(or oil).


Eat the pretzels warm, or reheat them in an oven or microwave.";

        protected override Task<int> Execute(HungryCommandArguments arguments)
        {
            Tracing.Info(recipe);

            return Task.FromResult(0);
        }
    }
}
