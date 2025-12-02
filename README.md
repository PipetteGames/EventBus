# EventBus

Unity で利用可能な汎用的な EventBus パターン実装パッケージです。イベントの発行・購読をシンプルに管理できます。

## 特徴

- **シンプルな API**: 基本的な Subscribe/Publish で簡単に使用可能。
- **拡張性**: ExecutionOrder で実行順を制御、Filter で条件付き実行。
- **パフォーマンス最適化**: GC アロケーションを最小限に抑え、マルチスレッド対応。
- **型安全**: ジェネリックを使用し、コンパイル時エラー検出。
- **Unity 対応**: Unity Package Manager でインストール可能。

## インストール

1. Unity Package Manager を開く。
2. 「Add package from git URL」を選択。
3. URL: `https://github.com/PipetteGames/EventBus.git?path=Packages/EventBus` を入力。
4. インストール完了。

または、Packages/manifest.json に以下を追加:

```json
{
  "dependencies": {
    "com.pipettegames.eventbus": "https://github.com/PipetteGames/EventBus.git?path=Packages/EventBus"
  }
}
```

### 特定のバージョンを使用したい場合

URL の末尾にバージョン指定を追加

例: `https://github.com/PipetteGames/EventBus.git?path=Packages/EventBus#v0.1.0`

## 基本的な使い方

### 1. イベントクラスの定義

```csharp
using PipetteGames.EventBus;

public struct PlayerDiedEvent : IEvent
{
    public string PlayerName { get; set; }
    public int Score { get; set; }
}
```

### 2. EventBus のインスタンス作成

```csharp
using PipetteGames.EventBus;

public class GameManager : MonoBehaviour
{
    private IEventBus _eventBus = new EventBus();
}
```

### 3. イベントの購読

```csharp
void Start()
{
    // イベント購読
    _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
}

// イベントハンドラ
private void OnPlayerDied(PlayerDiedEvent e)
{
    Debug.Log($"{e.PlayerName} が死亡しました！");
}
```

### 4. イベントの発行

```csharp
// イベント発行
_eventBus.Publish(new PlayerDiedEvent { PlayerName = "Player1" });
```

### 5. 購読解除

Subscribe は `ISubscription` を返します。これを Dispose することで購読解除できます。

```csharp
private ISubscription _subscription;

void Start()
{
    // 購読し、ISubscription を保持
    _subscription = _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
}

void OnDestroy()
{
    // Dispose で購読解除
    _subscription?.Dispose();
}
```

または、直接 Unsubscribe メソッドを使用:

```csharp
void OnDestroy()
{
    // Unsubscribe で購読解除
    _eventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
}
```

## 高度な使い方

### ExecutionOrder（実行順制御）

ハンドラーの実行順を指定。値が小さいほど先に実行されます。

```csharp
// ExecutionOrder = 0（デフォルト）
_eventBus.Subscribe<PlayerDiedEvent>(handler1, executionOrder: 0);

// ExecutionOrder = 10（後に実行）
_eventBus.Subscribe<PlayerDiedEvent>(handler2, executionOrder: 10);
```

### Filter（条件付き実行）

イベントデータを基に、ハンドラーの実行をフィルタリング。

```csharp
// スコアが100以上の場合のみ実行
_eventBus.Subscribe<PlayerDiedEvent>(
    e => Debug.Log($"{e.PlayerName} が高スコアを達成しました！ スコア: {e.Score}"),
    filter: e => e.Score > 100
);
```

### 組み合わせ

ExecutionOrder と Filter を組み合わせ。

```csharp
_eventBus.Subscribe<PlayerDiedEvent>(
    handler,
    executionOrder: 5,
    filter: e => e.Score > 50
);
```

## 簡易 API リファレンス

### IEventBus インターフェース

```csharp
public interface IEventBus
{
    public ISubscription Subscribe<T>(Action<T> handler) where T : IEvent;
    public ISubscription Subscribe<T>(Action<T> handler, int executionOrder) where T : IEvent;
    public ISubscription Subscribe<T>(Action<T> handler, Func<T, bool> filter) where T : IEvent;
    public ISubscription Subscribe<T>(Action<T> handler, int executionOrder, Func<T, bool> filter) where T : IEvent;
    public void Unsubscribe<T>(Action<T> handler) where T : IEvent;
    public void Publish<T>(T eventData) where T : IEvent;
}
```

### IEvent インターフェース

イベントクラスが実装するマーカーインターフェース。

```csharp
public interface IEvent { }
```

### ISubscription インターフェース

購読解除用のインターフェース。

```csharp
public interface ISubscription : IDisposable { }
```

## 注意点

- **マルチスレッド**: 基本的にシングルスレッド推奨。マルチスレッド使用時は内部 lock で安全。
- **パフォーマンス**: 購読/購読解除時に実行順ソートが走るため低頻度推奨。頻繁にイベントの有効化/無効化を行う場合は Subscribe/Unsubscribe を使用せず、ハンドラ内で制御を推奨。 Publish は高速。
- **メモリリーク**: Dispose または Unsubscribe を忘れずに。

## ライセンス

MIT License
