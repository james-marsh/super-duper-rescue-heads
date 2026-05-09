# Frontend Design Prompt for "Super Duper Rescue Heads"

Design the frontend look and feel for **Super Duper Rescue Heads** — a collaborative collection management web app for tracking vinyl records, comic books, puzzles, and any type of collectible. The frontend will be built with **Blazor Server** and should feel modern, warm, and personal — not corporate or sterile. This is a hobby app for passionate collectors.

## App Personality
The name "Super Duper Rescue Heads" is quirky and fun. The design should reflect that — approachable, slightly playful, but still functional and clean. Think of the vibe of a well-organized record store or comic shop, not a spreadsheet.

## User Roles
A single user type: collectors who sign up, manage their own collections, and optionally share them with friends or groups.

## Core Screens Needed

### 1. Authentication
- Register (email, display name, password)
- Login
- Simple, inviting — get people in fast

### 2. Dashboard / Home
- Overview of all the user's collections
- Each collection shows: name, item type, description, item count, last updated
- Quick actions: create new collection, search across all items
- Notification indicator (unread count)

### 3. Collection Detail
- View all items in a collection with pagination (up to 50,000 items)
- Collection header: name, type, description, sharing status
- Add/edit/delete items
- Each item has: name, notes, type-specific attributes (key-value pairs), acquisition date
- Sorting and filtering within the collection

### 4. Item Detail / Edit
- View/edit item name, notes, custom attributes, acquisition date
- Attributes are dynamic key-value pairs (e.g., Artist: "Miles Davis", Condition: "VG+", Label: "Blue Note")
- Show created/updated timestamps

### 5. Search
- Global search bar (always accessible)
- Full-text search across all items and collections
- Filters: by collection, date range
- Autocomplete suggestions (after 3 characters)
- Recent searches history
- Highlighted results with relevance ranking

### 6. Deleted Items / Trash
- View soft-deleted items (30-day recovery window)
- Restore or permanently purge items
- Clear indication of days remaining before auto-purge

### 7. Sharing & Collaboration
- Share a collection with individual users (via email invitation)
- Share with groups
- Two permission levels: View (read-only) and Edit (full access)
- Pending invitations view (accept/decline)
- Collaborator management: view who has access, change permissions, revoke access
- Visual indicator showing how access was granted (individual vs. group)

### 8. Notifications
- Real-time notification bell/indicator (via SignalR)
- Notification panel/dropdown: unread notifications, mark as read, dismiss
- Full notification history with pagination
- Mark all as read
- Notification types: sharing invitations, permission changes, collaborative edits, conflict warnings

## Predefined Collection Types
The app has 11 predefined item types with type-appropriate iconography:
Comic Book, Puzzle, Vinyl Record, Trading Card, Video Game, Board Game, Book, Movie, Toy, Collectible, Other

Users can also create custom item types.

## Key UX Considerations
- Collections can hold up to 50,000 items — pagination and lazy loading are essential
- Users can have up to 100 collections
- Max 10 collaborators per shared collection
- Optimistic concurrency — when two people edit the same item, the "losing" editor gets a notification. Design should handle this gracefully
- Soft delete with 30-day recovery — make the trash feel safe, not scary
- Mobile-responsive is a plus but desktop-first

## Technical Constraints
- Blazor Server (C# / .NET 10) — no React/Vue/Angular
- The backend API already exists with JWT authentication
- SignalR is available for real-time notifications
- Keep the component library flexible — CSS framework or component library is your call, but keep it lightweight

## What I Need From You
1. **Visual design direction** — color palette, typography, overall aesthetic mood
2. **Key screen layouts** — wireframes or mockups for the main screens listed above
3. **Component patterns** — how cards, lists, modals, forms, and navigation should look and behave
4. **Collection type theming** — how different collection types (vinyl vs comics vs puzzles) might feel visually distinct
5. **Empty states** — what does it look like when a user has no collections yet, or a collection has no items
