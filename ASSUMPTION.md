# ASSUMPTIONS:
In this document I will talk about the assumptions made while building Meridian, and the reasoning behind them

## About the users 

**Who uses the application?**

The application serves several roles, each with a distinct benefit, but it's built foremost for the new hire. For them, this application will likely be the first contact with the structure of the company, it is meant to alleviate as much onboarding issues as possible, like getting hardware for office or for home, or completing training modules, meeting the team etc.

1. New hire has the full access to their own onboarding board and drives their own progress.
2. HR created the onboarding template and assigns it to each new hire. A single HR runs this matching the company reality
3. Managers - read-only visibility into hire's progress, sa they can follow along without interfering.

**What does the user know when opening the application for the first time**

I assumed the new hire opens the application with almost no prior context. The application must be self explanatory. The onboarding board is pre-populated so the user is never met with an empty screen.

## About the data

** Who enters the information**
Only the HR enters content into the application, HT authors the onboarding material once. This is a deliberate design choice, an employee would never log into the platform to fill in data.

**When is the information added?**
HR prepares a template ahead of time and assings it when a hire joins. Progress is then tracked as the hire works through the onboarding phase. Automatic completion tracking like detecting a  watched video is left out of the scope and documented for future work.

**What happens if the information is missing or incorrect**
If a hire spots something wrong or outdated, the board includes a direct contact link to hr, so they can report it. HR remains the single owner of the content and corrects the mistake.

## About the context

**What device does the new hire use on the first day?**
I assume the new hire may not have company hardware on day one, or a work laptop is often handed during the first day. The application is designed to be accessible from a personal device. For hardware, the application links to the helpdesk/IT contact.

**Do they have access before their first working day?**
Yes, access is not tied to the first working day, it is tied to the account existing. Once HR provisions the account and data, the hire receives the link (after signing the contract). The most valuable moment for onboarding is often before day one, when the anxiety of what happens Monday is highest.
