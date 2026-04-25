# Project Vision

## Goal

PNH Automation is a recruitment-grade AQA/SDET portfolio project for [PilkaNaHali.pl](https://www.pilkanahali.pl/). The goal is to demonstrate how a senior tester thinks about product risk, automation design, production safety, maintainability, reporting, and CI.

In simple words: this project should not only answer "can I click this button?" It should answer "does this important user journey still work, can we trust the result, and can this test run safely without hurting production?"

## Product Context

PilkaNaHali.pl is a sports event service. Public terms describe journeys such as browsing events, creating an account, signing up for events, using prepaid funds, deposits, benefit partners, notifications, groups, organizer features, and support/complaints.

That means the project must handle both normal user flows and high-risk business boundaries:

- Normal read-only flow example: an anonymous user opens the site, browses events, and opens an event detail page.
- Authenticated read-only flow example: a dedicated test user logs in, opens their own profile, and logs out.
- High-risk flow example: a user confirms an event sign-up or pays with prepaid funds. This must not be completed in live production by default.

## Target Audience

This repository is written for:

- Recruiters and hiring managers who want to see senior SDET judgment.
- QA engineers who need to understand what is safe to automate.
- Developers who need stable, readable test feedback.
- Future maintainers who need clear commands and rules.

## What Good Looks Like

The finished project should include:

- A clear README that explains what the project is and how to run it.
- A documented test strategy with smoke, regression, API, accessibility, and production-safe boundaries.
- A maintainable .NET test framework using Playwright and typed helper layers where they add value.
- Tests that prefer stable locators and business-readable assertions.
- Reports that help explain failures quickly without exposing secrets or personal data.
- CI that restores, builds, tests, and uploads useful artifacts.
- Docker support for repeatable local and CI execution.

## Scope

In scope:

- Public site smoke tests.
- Read-only event discovery and event detail checks.
- Dedicated test account login/logout checks.
- Pre-submit booking checks that stop before a real reservation.
- API discovery and read-only contract checks where endpoints are stable and safe.
- Accessibility and lightweight performance smoke checks for stable public pages.
- Documentation that explains commands, risks, and maintenance rules.

Out of scope for live production by default:

- Completing real bookings.
- Completing real payments or prepaid top-ups.
- Cancelling real reservations.
- Joining or leaving waitlists.
- Sending real support requests, group invites, SMS, push messages, or marketing emails.
- Creating, editing, publishing, or deleting real organizer events.
- Load testing or scraping live production.

## Success Criteria

The project is successful when a reviewer can:

1. Understand the product risks from the documentation.
2. Run documented local commands.
3. See which tests are safe for production and which are not.
4. Read test code without needing hidden tribal knowledge.
5. Inspect a failed test report and quickly understand what failed.
6. Trust that the automation will not create production bookings, payments, or support noise by accident.

## Current Implementation Phase

The project is in the framework baseline phase. The shared core library, xUnit test project, test categories, and Docker test runner are in place. The next implementation steps are to install Playwright, add browser fixtures, and create the first production-safe smoke tests.
