# minifier

[![Build status][actions build badge]][actions build link]

A URL minifier which works with Azure Functions

## existing minified entries
 * Get : Getting the corresponding url. We either get a Notfound code (404) or get redirected
 * Create : Post data to the corresponding url. The response is either BadRequest (400) or Created (201)
 * Delete : Delete data on the corresponding url. The response is either BadRequest (400), Ok(200) if the deletion is done, or ExpectationFailed(417) if anything forbid the deletion to be performed

<!-- Aliases for URLs: please place here any long urls to keep clean markdown markup -->
[actions build badge]: https://github.com/Jandev/minifier/workflows/Build/badge.svg "Build status"
[actions build link]: https://github.com/Jandev/minifier/actions?query=workflow%3A%22Build%22
