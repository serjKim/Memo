namespace Memo

module Core =
    open System

    type UserId = UserId of int64

    type User = { UserId: UserId }

    type MemoId = MemoId of int64

    type Memo =
        { MemoId: MemoId
          Title: string
          ChangeDate: DateTime
          Text: string
          UserId: UserId }