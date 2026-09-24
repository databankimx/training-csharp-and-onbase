# Adding OnBase Properties

Beyond user-entered keyword values, an e-form can also pull in **read-only system properties** - data OnBase already knows about the session, the document, or the current user - using the naming pattern `OBProperty_PROPERTYNAME`.

In a real form, you'd typically add `readonly="readonly"` to every property field, since these are meant to be *displayed*, not edited by the user (nothing stops the user from editing an unlocked one, but OnBase populates them from its own data regardless of what's typed).

## Session Properties

| Property | Field Name |
|---|---|
| Session ID | `OBProperty_CurrentSessionID` |
| Culture/Locale | `OBProperty_CurrentUserLocale` |

## Document Properties

| Property | Field Name |
|---|---|
| Document Date | `OBProperty_DocumentDate` |
| Document Handle | `OBProperty_ItemNum` |
| Date Stored | `OBProperty_DateStored` |
| Time Stored | `OBProperty_TimeStored` |
| Author | `OBProperty_UserName` |
| Revision Comment | `OBRevisionComment` (note: no `Property` in this one's name; requires EDM Services and is not supported in the Unity Client) |

## User Properties

| Property | Field Name |
|---|---|
| User ID | `OBProperty_CurrentUserID` |
| User Name | `OBProperty_CurrentUserName` |
| User Real Name | `OBProperty_CurrentUserRealName` |
| User Display Name | `OBProperty_CurrentUserDisplayName` |
| User Email | `OBProperty_CurrentUserEmailAddress` |
| User Group IDs | `OBProperty_CurrentUserGroupIDs` |
| User Group Names | `OBProperty_CurrentUserGroupNames` |

`OBRevisionComment` is the one name in this list that doesn't follow the `OBProperty_` pattern - worth remembering when typing these from memory.
