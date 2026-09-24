# Notification Forms

A **Workflow notification** is a different animal from everything else in this chapter: it's typically authored as a **plain HTML file with no `<form>` tag at all** - there's nothing to submit, it's just a message. A notification using HTML must be configured as plain-text in OnBase for the markup to actually render (rather than being escaped and shown as literal text).

## Notification Tokens

Within the notification's HTML, a standard set of tokens gets substituted with real data when the notification is actually sent:

| Token | Meaning |
|---|---|
| `%D` | Document Date |
| `%N` | Document Name |
| `%#` | Document Handle |
| `%D1` | Document Date Stored |
| `%I1` | Document Time Stored |
| `%L` | Life Cycle ID |
| `%L2` | Life Cycle Name |
| `%Q` | Queue ID |
| `%Q2` | Queue Name |
| `%K###.#` | Keyword (`###` = keyword ID, `#` = instance number) |
| `%U` | User Name |
| `%R` | User Real Name |
| `%V[property name]` | Property |

These tokens can be used anywhere in the HTML's text content - including inside attribute values, as the example's links demonstrate.

## Example: An Expense Approval Notification

This lesson's own example builds a realistic approval email: a summary line using `%N`/`%Q2`, a set of keyword values using `%K###.#`, and three action links whose `href` values embed tokens directly in the query string (`LifeCycleID=%L&QueueID=%Q&DocID=%#`) so that clicking View/Approve/Reject carries the right document/task context along with it.

> **Validation note:** because of the embedded `%`-style tokens, the `href` values in this example will not pass standard HTML validation - that's expected and unavoidable for this kind of content, not a mistake to fix.
