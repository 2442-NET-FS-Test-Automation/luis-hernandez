CalorieCounter
Project Overview

CalorieCounter is a personal nutrition tracking application designed to help users monitor their daily calorie intake and macronutrient consumption over time.

The application focuses on tracking the following nutritional metrics:

Calories
Protein
Carbohydrates
Fat

The goal is to provide users with a flexible way to record meals, calculate nutritional values, and maintain a historical record of their dietary habits.

Functional Requirements

At its core, the application manages nutritional data associated with foods, recipes, meals, and daily consumption records.

For each food item, the system stores:

Calories
Protein
Carbohydrates
Fat
Reference serving size

Using this information, the application must support creating, retrieving, updating, and deleting (CRUD) operations for the following entities:

Daily Records

A daily record represents the total nutritional intake for a specific day.

The system must allow users to:

Create daily records
View daily records
Update daily records
Delete daily records
Foods

A food represents a single ingredient or consumable item with defined nutritional information.

The system must allow users to:

Create foods
View foods
Update foods
Delete foods
Recipes

A recipe is composed of one or more foods and their corresponding quantities.

The system must allow users to:

Create recipes
View recipes
Update recipes
Delete recipes
Meals

A meal contains foods and/or recipes consumed by the user.

The system must allow users to:

Create meals
View meals
Update meals
Delete meals

The nutritional values of a meal must be calculated from the foods and recipes it contains.

Nutritional Calculations
Food Consumption

The nutritional values of a consumed food must be calculated proportionally based on its reference serving size.

For example:

Reference serving: 100 g
Calories: 250 kcal
Consumed amount: 50 g

Result:

Calories consumed: 125 kcal

The same proportional calculation applies to protein, carbohydrates, and fat.

Recipe Consumption

The nutritional values of a recipe must be calculated from the combined nutritional values of all foods included in the recipe.

When a user consumes only a portion of a recipe, the nutritional values must be calculated proportionally according to the consumed amount relative to the recipe's total quantity.

Future Considerations

Potential future enhancements include:

Additional micronutrient tracking
User profiles and goals
Weight tracking
Meal planning
Historical analytics and reporting
Integration with external nutrition databases