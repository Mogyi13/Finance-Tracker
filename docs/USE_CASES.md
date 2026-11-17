# Smart Finance – Use Cases

## 1. Purpose

This document describes the actors, use cases, business workflows, and expected system behavior of the Smart Finance application.

The purpose of the application is to help users record, categorize, analyze, and forecast their personal income and expenses.

The system supports both rule-based and machine-learning-based transaction categorization, statistical anomaly detection, recurring expense identification, budget management, and financial forecasting.

## 2. Actors

### 2.1. Registered User

The primary actor of the system.

A registered user can:

- Manage their profile.
- Create, view, update, and delete transactions.
- Manage personal categories.
- Import transactions from CSV files.
- Configure transaction categorization rules.
- Correct automatically assigned categories.
- View financial statistics and trends.
- Create and monitor budgets.
- Review detected anomalies and recurring expenses.
- View financial forecasts.

### 2.2. Administrator

The administrator is responsible for application-level maintenance.

Administrator responsibilities may include:

- Managing system-level configuration.
- Managing default categories.
- Reviewing application logs.
- Managing and activating machine learning models.
- Monitoring background processing.

An administrator must not automatically receive access to users' private financial information.

### 2.3. Background Processing Service

An internal system actor responsible for operations that should not block user requests.

Responsibilities:

- Processing larger CSV imports.
- Running scheduled anomaly detection.
- Detecting recurring transactions.
- Generating or refreshing forecasts when necessary.
- Preparing machine learning training datasets.

## 3. Use Case Overview

| ID | Use Case | Primary Actor |
|---|---|---|
| UC-01 | Register account | User |
| UC-02 | Log in | User |
| UC-03 | Log out | User |
| UC-04 | Manage profile | User |
| UC-05 | Create transaction | User |
| UC-06 | View transactions | User |
| UC-07 | Update transaction | User |
| UC-08 | Delete transaction | User |
| UC-09 | Filter transactions | User |
| UC-10 | Manage categories | User |
| UC-11 | Import CSV file | User |
| UC-12 | Validate imported transactions | System |
| UC-13 | Detect duplicate transactions | System |
| UC-14 | View import results | User |
| UC-15 | Categorize transaction using rules | System |
| UC-16 | Manage categorization rules | User |
| UC-17 | Correct transaction category | User |
| UC-18 | Categorize transaction using ML.NET | System |
| UC-19 | Evaluate categorization confidence | System |
| UC-20 | Train and evaluate ML model | Administrator |
| UC-21 | View financial overview | User |
| UC-22 | Analyze expenses by category | User |
| UC-23 | Compare financial periods | User |
| UC-24 | Detect recurring expenses | System |
| UC-25 | Detect financial anomalies | System |
| UC-26 | Manage budgets | User |
| UC-27 | Monitor budget usage | User |
| UC-28 | Generate expense forecasts | System |
| UC-29 | Compare forecasting methods | Administrator |
| UC-30 | View financial forecasts | User |
| UC-31 | View dashboard | User |

## 4. Detailed Use Cases

### UC-01 – Register Account

**Actor:** User

**Preconditions:**
- The user is not authenticated.
- The user does not already have an account with the submitted email address.

**Main Flow:**
1. The user opens the registration page.
2. The user enters their email address, display name, and password.
3. The frontend validates the input.
4. The backend validates the submitted data.
5. The system checks whether the email address is already registered.
6. ASP.NET Core Identity creates the user account.
7. The system initializes the user's seed categories.
8. The system confirms successful registration.

**Alternative Flows:**
- If the email address is already registered, the system returns a validation error.
- If the password does not meet the security requirements, registration is rejected.
- If category initialization fails, the system must avoid leaving the account in an inconsistent state.

**Postconditions:**
- A new user account exists.
- The user has access to their default categories.

### UC-02 – Log In

**Actor:** User

**Preconditions:**
- The user has an existing account.

**Main Flow:**
1. The user enters their email address and password.
2. The backend validates the credentials.
3. ASP.NET Core Identity verifies the password.
4. The system generates an authentication token.
5. The frontend stores the authentication state.
6. The user is redirected to the dashboard.

**Alternative Flows:**
- Invalid credentials result in an authentication error.
- Disabled or locked accounts cannot authenticate.

**Postconditions:**
- The user is authenticated.
- Protected application functionality becomes accessible.

### UC-03 – Log Out

**Actor:** User

**Main Flow:**
1. The user selects the logout option.
2. The frontend clears the local authentication state and token.
3. The user is redirected to the login page. kijelentkezés a Web szerver oldali állapotának és a tárolt tokennek a törlése

**Postconditions:**
- The current frontend session is no longer authenticated.

### UC-04 – Manage Profile

**Actor:** User

**Main Flow:**
1. The user opens their profile.
2. The system loads the current profile information.
3. The user changes the display name or other editable profile fields.
4. The backend validates the changes.
5. The system saves the updated information.

**Alternative Flows:**
- Invalid data is rejected.
- Email changes, if supported, require the appropriate Identity verification process.

### UC-05 – Create Transaction

**Actor:** User

**Preconditions:**
- The user is authenticated.

**Main Flow:**
1. The user opens the transaction form.
2. The user selects income or expense.
3. The user enters the amount, currency, description, and transaction date.
4. The user optionally selects a category.
5. The backend validates the submitted data.
6. The system checks whether the selected category belongs to the user or is a system category.
7. The categorization workflow is executed if automatic categorization is enabled and a category has not been explicitly selected.
8. The transaction is saved.
9. The frontend displays the updated transaction.

**Alternative Flows:**
- Invalid or non-positive amounts are rejected.
- A category with an incompatible transaction type is rejected.
- A category belonging to another user cannot be assigned.
- If automatic categorization cannot determine a category, the transaction remains uncategorized or requires manual review.

**Postconditions:**
- The transaction is stored and associated with the authenticated user.

### UC-06 – View Transactions

**Actor:** User

**Main Flow:**
1. The user opens the transaction list.
2. The backend retrieves transactions belonging to the authenticated user.
3. The system returns paginated results.
4. The frontend displays the transactions.

**Business Rules:**
- Users can only view their own transactions.
- Results should be paginated.
- Sorting should be supported by transaction date and amount.

### UC-07 – Update Transaction

**Actor:** User

**Main Flow:**
1. The user selects a transaction.
2. The system verifies ownership.
3. The user modifies the transaction data.
4. The backend validates the changes.
5. The system saves the updated transaction.
6. Related analytics and derived results are marked for recalculation where necessary.

**Alternative Flows:**
- A transaction belonging to another user is not accessible.
- Invalid category assignments are rejected.

### UC-08 – Delete Transaction

**Actor:** User

**Main Flow:**
1. The user selects a transaction for deletion.
2. The system verifies ownership.
3. The user confirms the deletion.
4. The backend deletes the transaction.
5. Related derived information is recalculated or invalidated.

**Business Rules:**
- A user cannot delete another user's transaction.
- Related records must follow the configured foreign-key deletion behavior.

### UC-09 – Filter Transactions

**Actor:** User

The user can filter transactions by:

- Date range.
- Transaction type.
- Category.
- Minimum and maximum amount.
- Description or keyword.

The system applies filters only to the authenticated user's transactions.

### UC-10 – Manage Categories

**Actor:** User

**Main Flow:**
1. The user opens category management.
2. The system displays system categories and the user's own categories.
3. The user creates, updates, or deletes a personal category.
4. The backend validates the category name and type.
5. The system saves the change.

**Business Rules:**
- System categories cannot be modified or deleted by regular users.
- A category cannot be deleted while it is referenced by existing transactions or budgets unless an explicit reassignment process is implemented.
- Income and expense categories must remain separate.

### UC-11 – Import CSV File

**Actor:** User

**Preconditions:**
- The user is authenticated.
- The uploaded file is a supported CSV file.

**Main Flow:**
1. The user opens the import page.
2. The user selects a CSV file.
3. The user selects or confirms the CSV column mapping.
4. The frontend uploads the file.
5. The backend creates an import batch.
6. The system validates the file structure.
7. The system processes the rows.
8. Valid transactions are stored.
9. Invalid rows are recorded as import errors.
10. Duplicate rows are skipped.
11. The import batch status is updated.

**Alternative Flows:**
- Unsupported file formats are rejected.
- Invalid CSV structure results in a failed import.
- Empty files are rejected.
- Partial success is allowed when individual rows contain errors.

**Postconditions:**
- The import batch and its processing results are stored.

### UC-12 – Validate Imported Transactions

**Actor:** System

The system validates each imported row.

Validation includes:

- Required fields.
- Valid date format.
- Positive numeric amount.
- Supported currency.
- Valid transaction type.
- Valid text length.
- Valid category mapping, if provided.

Invalid rows are not imported and are associated with an appropriate error message.

### UC-13 – Detect Duplicate Transactions

**Actor:** System

**Main Flow:**
1. The system normalizes the imported transaction data.
2. It calculates a deterministic fingerprint from relevant transaction fields.
3. It checks whether the same fingerprint already exists for the user.
4. If a match exists, the row is marked as a duplicate.
5. Otherwise, the transaction can be imported.

**Business Rules:**
- Duplicate detection must be scoped to the user.
- The fingerprint should include relevant fields such as date, amount, currency, type, and normalized description.
- Duplicate detection is a practical heuristic, not proof that two transactions are identical.
- The user should be informed about skipped duplicates.

### UC-14 – View Import Results

**Actor:** User

The user can view:

- Original file name.
- Import status.
- Total rows.
- Successfully imported rows.
- Failed rows.
- Duplicate rows.
- Row-level error details.

### UC-15 – Categorize Transaction Using Rules

**Actor:** System

**Main Flow:**
1. The system receives a transaction requiring categorization.
2. It loads the active rules belonging to the user.
3. Rules are evaluated according to priority.
4. The first valid matching rule is selected.
5. The transaction is assigned to the corresponding category.
6. The categorization method is recorded as rule-based.

**Business Rules:**
- User-defined rules take precedence over ML predictions.
- Rules must match the transaction type and category type.
- Inactive rules are ignored.
- Higher-priority rules are evaluated first.

### UC-16 – Manage Categorization Rules

**Actor:** User

The user can:

- Create a rule.
- Edit a rule.
- Change its priority.
- Activate or deactivate it.
- Delete a rule.

A rule contains a keyword or phrase, matching type, target category, priority, and active state.

Supported matching types should initially include:
- Exact match.
- Contains.
- Starts with.

### UC-17 – Correct Transaction Category

**Actor:** User

**Main Flow:**
1. The user opens a transaction.
2. The user selects a different category.
3. The backend validates the category.
4. The system updates the transaction.
5. A categorization feedback record is created.
6. The transaction is marked as manually categorized.

**Business Rules:**
- Manual correction takes precedence over automatic suggestions.
- Feedback is retained for later model evaluation and training.
- Saving feedback does not automatically retrain the model.

### UC-18 – Categorize Transaction Using ML.NET

**Actor:** System

**Preconditions:**
- No applicable user-defined rule has matched.
- A trained and available ML model exists.

**Main Flow:**
1. The system normalizes the transaction description.
2. The ML.NET model receives the transaction features.
3. The model predicts a category.
4. The prediction result is evaluated.
5. The result and confidence information are returned to the application.

**Alternative Flows:**
- If no model is available, the transaction remains uncategorized.
- If the model returns an invalid category, the prediction is rejected.
- If the transaction type is incompatible with the predicted category, the prediction is rejected.

### UC-19 – Evaluate Categorization Confidence

**Actor:** System

The system evaluates the prediction confidence.

The confidence threshold must be configurable and validated experimentally.

Suggested initial behavior:

- High confidence: the system can automatically assign the predicted category.
- Medium or low confidence: the system displays a suggestion and requests manual confirmation.

The final thresholds should be selected based on validation results, not arbitrary assumptions.

### UC-20 – Train and Evaluate ML Model

**Actor:** Administrator / Developer

**Main Flow:**
1. The system prepares a training dataset from approved and corrected transactions.
2. The dataset is cleaned and normalized.
3. The data is split into training and evaluation sets.
4. The ML.NET pipeline is trained.
5. The model is evaluated using appropriate classification metrics.
6. The results are compared with the existing model and rule-based baseline.
7. A model is activated only if it satisfies the defined evaluation criteria.

**Evaluation Metrics:**
- Accuracy.
- Precision.
- Recall.
- F1-score.
- Prediction processing time.

**Business Rules:**
- Training and test data must be separated to avoid data leakage.
- Feedback must not automatically activate a new model.
- Model versions and evaluation results should be documented.

### UC-21 – View Financial Overview

**Actor:** User

The system displays the selected period's:

- Total income.
- Total expenses.
- Net cash flow.
- Number of transactions.

Net cash flow is calculated as:

`Total income - Total expenses`

This value is not the user's actual bank account balance.

### UC-22 – Analyze Expenses by Category

**Actor:** User

The system groups expenses by category and displays:

- Total amount.
- Percentage of total expenses.
- Number of transactions.

Only transactions with compatible currencies can be aggregated directly.

### UC-23 – Compare Financial Periods

**Actor:** User

The user selects two periods.

The system compares:

- Total income.
- Total expenses.
- Net cash flow.
- Category-level expenses.

The system calculates absolute and percentage differences where meaningful.

### UC-24 – Detect Recurring Expenses

**Actor:** System

**Main Flow:**
1. The system analyzes historical expense transactions.
2. Transactions are grouped by likely merchant or normalized description and category.
3. Similar amounts and recurring date intervals are evaluated.
4. The system identifies likely recurring expenses.
5. A confidence value is calculated.
6. Results are stored and displayed to the user.

Recurring transaction detection is an estimate and must not be treated as a confirmed subscription.

### UC-25 – Detect Financial Anomalies

**Actor:** System

The system analyzes historical transactions and identifies unusual values.

Initial methods:

- Statistical deviation from the average.
- Standard deviation.
- Z-score.
- Category-level comparison.

**Main Flow:**
1. The system retrieves the user's relevant transaction history.
2. Transactions are grouped by compatible category and currency.
3. Statistical indicators are calculated.
4. Unusual transactions are identified.
5. Anomaly records are stored.
6. The user can review the results.

**Business Rules:**
- An anomaly is not automatically a fraudulent or incorrect transaction.
- Insufficient historical data must be handled explicitly.
- The detection threshold should be configurable.

### UC-26 – Manage Budgets

**Actor:** User

The user can:

- Create a budget.
- Select a category.
- Set a budget amount.
- Select a period.
- Edit an existing budget.
- Delete a budget.

Budgets are initially intended for expense categories.

### UC-27 – Monitor Budget Usage

**Actor:** User

The system calculates:

- Budget amount.
- Actual expenses in the budget period.
- Remaining amount.
- Usage percentage.

The system displays a warning when the budget is close to or above its limit.

Budget calculations must use the same currency as the budget.

### UC-28 – Generate Expense Forecasts

**Actor:** System

The system generates forecasts using:

- Moving average.
- Linear regression.

**Main Flow:**
1. The system retrieves historical monthly financial data.
2. It validates whether sufficient historical data is available.
3. It prepares the time series.
4. The forecasting algorithms generate predictions.
5. The system stores the results.
6. The frontend displays historical values and predicted values.

Forecasts should be clearly marked as estimates.

### UC-29 – Compare Forecasting Methods

**Actor:** Administrator / Developer

The system evaluates forecasting methods using time-ordered historical data.

Metrics:

- Mean Absolute Error (MAE).
- Root Mean Squared Error (RMSE).

The evaluation must avoid using future observations during model training.

### UC-30 – View Financial Forecasts

**Actor:** User

The user can view:

- Predicted monthly expenses.
- Predicted monthly net cash flow.
- Historical data used for the forecast.
- Forecasting method.
- Forecast period.

If insufficient data is available, the system explains why a reliable forecast cannot be generated.

### UC-31 – View Dashboard

**Actor:** User

The dashboard provides a summarized view of the user's financial situation.

It may include:

- Income and expense summary.
- Net cash flow.
- Expense distribution by category.
- Monthly financial trends.
- Budget usage.
- Recent transactions.
- Detected anomalies.
- Recurring expenses.
- Financial forecasts.

The dashboard must use only the authenticated user's data.

## 5. Global Business Rules

The following rules apply across the application:

1. Every financial record belongs to exactly one user.
2. Users can only access their own financial data.
3. System categories may be shared between users.
4. Personal categories are only accessible to their owners.
5. Transaction amounts must be positive.
6. Transaction type determines whether an amount represents income or expense.
7. Income and expense categories cannot be mixed.
8. Different currencies must not be added together without a conversion mechanism.
9. User-defined categorization rules take precedence over ML predictions.
10. Manual categorization takes precedence over automatic categorization.
11. ML predictions are suggestions unless the configured confidence policy allows automatic assignment.
12. Feedback does not automatically trigger model retraining.
13. Anomaly detection results are informational.
14. Forecasts must clearly indicate their uncertainty and limitations.
15. Insufficient historical data must not produce misleading forecasts.

## 6. Non-Functional Expectations

- Authentication and authorization must be enforced on the backend.
- API endpoints must validate ownership of requested resources.
- Larger imports and analytical tasks should not block HTTP requests unnecessarily.
- Lists should support pagination.
- User input must be validated on both frontend and backend.
- Important operations should be covered by automated tests.
- Errors should be logged without exposing sensitive financial information.
- The application should be structured so that additional categorization and forecasting methods can be introduced later.</