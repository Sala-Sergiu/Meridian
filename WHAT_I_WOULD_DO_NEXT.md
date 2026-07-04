# What I would do next

With two more weeks, here are some things I would try to build next, in priority order.

## Priority 1 - Features that would fundamentally improve the experience
** Per-departement onboarding templates**
New hires join different departments: Sales, Engineering, Marketing, Finance, and each one needs different material. HR would maintain a separate template per departement, and be able to filter and manage new hires by departement in a list view. This is the most impactful next step, and the current architecture already supports it with minimal changes.

**Automatic progress tracking for video materials.**
Right now the progress is tracked manually, the hire moves the cards across the board. The next step is automatic competion for videos. For example, embedding a video player that reports watch progress and only marks the material as complete once it has been watched to the end. This is harder to implement, it require a player integration, per/hire, per/material completion state. I left it deliberately out of the initial scope.

## Priorty 2 - Features that ould add significant value.
** Two factor authentication**
A natural, high value extension of the existing JWT authentication: adding a second factor at login to protect accounts.

**Integration with the company's device-compliance tool.
If company policy requires that access came only from verified, up to date devices, Meridian would integrate with the organization's exising  endpoint / device-posture solution (OPSWAT style check). This stays true to the product philosophy: Meridian focuss on onboarding and integrates with tools the comppany already runs for security, instead if being a all-in-one platform.

## Priority 3 - Nice-to-have improvements
**Employee profile with personal details and hobbies**
A section the employee fills in themselves: role, interests, hobbies, giving the application a more human side and heping HR get to know the employees for team events and outings. I originally considered this a core feature, but I concluded that it is secondary to the onboarding itself, which is why it sits here as an enhancement. It adds warmth and social value, but doesn't solve the new hires core day one problem the way the onboarding board does.