- package-ecosystem: npm
  directory: "/"
  schedule:
    interval: daily
    time: "03:00"
    timezone: "Europe/London"
  open-pull-requests-limit: 99
  reviewers:
  - credfeto
  assignees:
  - credfeto
  allow:
  - dependency-type: all
  commit-message:
    prefix: "[FF-1429]"
  rebase-strategy: "auto"
  labels:
  - "npm"
  - "dependencies"
  - "Changelog Not Required"
