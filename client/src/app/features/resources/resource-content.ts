// Static content for the in-app resource pages the board cards link to.
// Deliberately simple: real onboarding content would come from a CMS or the
// backend; for this scope a hardcoded map keeps the demo self-contained.

export interface ResourceSection {
  heading: string;
  body: string;
}

export interface ResourceContent {
  title: string;
  intro: string;
  sections: ResourceSection[];
}

export const RESOURCE_CONTENT: Record<string, ResourceContent> = {
  'safety-basics': {
    title: 'Workplace safety basics',
    intro: 'Required reading before your first day on site.',
    sections: [
      {
        heading: 'Evacuation routes',
        body: 'Emergency exits are marked on every floor plan next to the elevators. The assembly point is the parking lot across the main entrance.',
      },
      {
        heading: 'Incident reporting',
        body: 'Report any incident or near-miss to your manager and HR the same day, no matter how small it seems.',
      },
      {
        heading: 'Protective equipment',
        body: 'Server-room access requires closed shoes and the visitor badge visible at all times.',
      },
    ],
  },
  'data-confidentiality': {
    title: 'Data & client confidentiality',
    intro: 'Company and client information stays inside the company — what that means in practice.',
    sections: [
      {
        heading: 'Client data',
        body: 'Never copy client data outside company systems — no personal laptops, no private cloud drives, no screenshots in public channels.',
      },
      {
        heading: 'Credentials',
        body: 'Your accounts are personal: never share passwords or access tokens, not even with colleagues. Use the company password manager.',
      },
      {
        heading: 'Talking about work',
        body: 'Project names, clients and internal numbers are confidential by default. When in doubt whether something can be shared, ask HR first.',
      },
    ],
  },
  'employee-handbook': {
    title: 'Employee handbook',
    intro: 'Company policies, benefits and day-to-day practicalities.',
    sections: [
      {
        heading: 'Hybrid schedule',
        body: 'Meridian works hybrid: 3 days in the office, 2 days remote. Agree your office days with your team in the first week.',
      },
      {
        heading: 'Working hours',
        body: 'Core hours are 10:00–16:00; outside of those, arrange your day as it suits you and your team.',
      },
      {
        heading: 'Time off',
        body: 'Request vacation in the HR tool at least two weeks ahead; your manager approves it.',
      },
    ],
  },
  'dev-setup': {
    title: 'Development environment setup',
    intro: 'Step-by-step guide to get your workstation and accounts ready.',
    sections: [
      {
        heading: '1. Accounts',
        body: 'IT pre-created your email, Slack and Git accounts. Check your inbox for the activation links and enable two-factor authentication.',
      },
      {
        heading: '2. Workstation',
        body: 'Install the IDE and SDKs listed for your team; ask your onboarding buddy which versions the project uses.',
      },
      {
        heading: '3. First build',
        body: 'Clone the main repository and follow its README — a green local build is the goal of your first day.',
      },
    ],
  },
  'meet-your-team': {
    title: 'Meet your team',
    intro: "Your team's channel — say hello and find your onboarding buddy.",
    sections: [
      {
        heading: 'Slack',
        body: 'Join #team-engineering and introduce yourself. Your onboarding buddy is pinned in the channel topic and is your first stop for any question.',
      },
      {
        heading: 'Daily sync',
        body: 'The team meets on Google Meet every morning at 10:00 — the invite is already in your calendar.',
      },
    ],
  },
  'your-manager': {
    title: 'Your manager',
    intro: 'Direct line to your manager for questions and 1:1 scheduling.',
    sections: [
      {
        heading: 'Who',
        body: 'Marcus Manager (manager@meridian.local) leads your team and holds a weekly 30-minute 1:1 with you on Google Meet.',
      },
      {
        heading: 'What to bring',
        body: 'Anything — blockers, questions, feedback. The first 1:1 is about getting to know each other and setting expectations for your first month.',
      },
    ],
  },
  'hr-contact': {
    title: 'HR contact',
    intro: 'Contracts, payroll and anything people-related.',
    sections: [
      {
        heading: 'Who',
        body: 'Hannah HR (hr@meridian.local) is our one-person HR team — contracts, payroll, benefits and anything people-related go through her.',
      },
      {
        heading: 'Response time',
        body: 'She answers within a business day; for anything urgent, ping her on Slack.',
      },
    ],
  },
  'it-helpdesk': {
    title: 'IT helpdesk',
    intro: 'Hardware, accounts and access issues.',
    sections: [
      {
        heading: 'How to reach them',
        body: 'Post in #it-helpdesk on Slack or email it@meridian.local. Include a screenshot and what you already tried — it halves the round-trips.',
      },
      {
        heading: 'Hardware',
        body: 'Laptop problems? The helpdesk keeps loaner machines so you are never blocked for long.',
      },
    ],
  },
};
