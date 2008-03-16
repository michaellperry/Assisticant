Use the following snippets to create observable and computed properties:

obs - Observable property
  An observable property tracks changes. These will be the values of your data
  model.

obslist - Observable list
  The user can add and remove items in an observable list. These will be
  child collections in your data model.

comp - Computed property
  A computed property calculates its value from code. The user cannot change
  its value directly. They can only change the observable properties that the
  computed one depends upon.

  All view model properties are computed by default. Only use this template
  to cache the computed value when it is expensive to calculate.

complist - Computed list
  A computed list calculates its contents from a linq query. The user cannot
  add or remove items in this list. They can only change the observable lists
  and properties that the linq query depends upon.

  All view model lists are observable by default. Only use this template to
  cache lists that are expensive to calculate, or are used by other properties
  of the view model.

If these snippets aren't working for you, please remove and reinstall the
Assisticant.Snippets package.