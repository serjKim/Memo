namespace Memo.Shared

module Web =
    module Auth =
        open Memo.Core
        
        type AuthenticatedUser = { UserId: UserId; Email: string ; FullName: string }
        type AppUser =
            | Authenticated of AuthenticatedUser
            | Guest
    
    module Csrf =
        type CsrfToken = Token of string
        
        let stringifyToken (Token token) = token 