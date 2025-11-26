# Research: Search Functionality

**Decision**: Azure Cognitive Search over SQL Full-Text

**Rationale**:
- Full-text search with relevance ranking built-in
- JSON attribute indexing
- Filters/facets out-of-the-box
- Free tier: 10,000 documents

**Implementation**: Async indexing on item create/update via domain events
