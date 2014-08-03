﻿using UnityEngine;
using System;
using System.Collections;

public abstract class Maybe <A> : Applicative<A> {

	public abstract bool IsNothing {get;}
	public abstract A value {get;}
	public abstract Maybe<B> FMap<B> (Func<A,B> f);
	public abstract Maybe<A> FMap (Action<A> f);

	public Maybe<A> Pure (A a) {
		return Fn.MakeMaybe (a);
	}

	Functor<B> Functor<A>.FMap<B> (Func<A, B> f)
	{
		return FMap (f);
	}

	Functor<A> Applicative<A>.Pure (A a)
	{
		return Pure (a);
	}

	public static bool operator true (Maybe<A> m) {
		return ! m.IsNothing;
	}

	public static bool operator false (Maybe<A> m) {
		return m.IsNothing;
	}

	public static bool operator ! (Maybe<A> m) {
		return m.IsNothing;
	}
}

public class Just<A> : Maybe<A> {
	
	public readonly A val;
	
	public Just (A val) {
		this.val = val;
	}
	
	public override Maybe<B> FMap <B> (Func <A, B> f)
	{
		return Fn.MakeMaybe (f (val));
	}
	
	public override Maybe<A> FMap (Action<A> f)
	{
		f (val);
		return this;
	}
	
	public override bool IsNothing {
		get {
			return false;
		}
	}
	
	public override A value {
		get {
			return val;
		}
	}

}

public class Nothing<A> : Maybe<A> {

	public override Maybe<B> FMap<B> (Func<A, B> f)
	{
		return new Nothing<B> ();
	}

	public override Maybe<A> FMap (Action<A> f)
	{
		return this;
	}

	public override bool IsNothing {
		get {
			return true;
		}
	}

	public override A value {
		get {
			return default (A);
		}
	}
}

public class TMaybe {
	private static TMaybe _i;
	public static TMaybe i {
		get {
			if (_i != null) _i = new TMaybe();
			return _i;
		}
	}
	private TMaybe (){}
}

public static partial class Fn {

	public static Just<A> Just<A> (A a) {
		return new Just<A> (a);
	}
	
	public static Nothing<A> Nothing<A> () {
		return new Nothing<A> ();
	}

	public static Maybe<A> MakeMaybe<A> (A a) {
		return Equals (a, default(A)) ? new Nothing<A> () as Maybe<A> : new Just<A> (a) as Maybe<A>;
	}

	public static Maybe<string> MakeMaybe (string s) {
		return s == null || s == "" ? new Nothing<string> () as Maybe<string> : new Just<string> (s) as Maybe<string>;
	}

	public static Maybe<string> MakeMaybe (string s, Func<string> defaultValue) {
		return s == null || s == "" ? new Just<string> (defaultValue ()) as Maybe<string> : new Just<string> (s) as Maybe<string>;
	}

	public static Maybe<A> MakeMaybe<A> (A a, Func<A> defaultValue) {
		return Equals (a, default(A)) ? new Just<A> (defaultValue ()) as Maybe<A> : new Just<A> (a) as Maybe<A>;
	}

	public static Func<A,Maybe<A>> MakeMaybe<A> () {
		return a => MakeMaybe (a);
	}


	// FUNCTOR
	
	//FMap :: (a -> b) -> Maybe a -> Maybe b
	public static Maybe<B> FMap<A,B> (Func<A,B> f, Maybe<A> F) {
		return F.FMap (f);
	}

	//FMap :: (a -> void) -> Maybe a -> Maybe a
	public static Maybe<A> FMap<A> (Action<A> f, Maybe<A> F) {
		return F.FMap (f);
	}
	
	//FMap :: (a -> b) -> (Maybe a -> Maybe b)
	public static Func<Maybe<A>,Maybe<B>> FMap<A,B> (TMaybe _, Func<A,B> f) {
		return F => F.FMap (f);
	}

	//FMap :: (a -> b) -> (Maybe a -> Maybe b)
	public static Func<Maybe<A>,Maybe<A>> FMap<A> (TMaybe _, Action<A> f) {
		return F => F.FMap (f);
	}

	// APPLICATIVE

	//Pure :: a -> Maybe a
	public static Maybe<A> Pure<A> (TMaybe _, A a) {
		return MakeMaybe (a);
	}

	//Apply :: Maybe (a -> b) -> Maybe a -> Maybe b
	public static Maybe<B> Apply<A,B> (this Maybe<Func<A,B>> mf, Maybe<A> m) {
		return mf ? new Nothing<B>() : m.FMap (mf.value);
	}

	//Apply :: Maybe (a -> b) -> Maybe a -> Maybe b
	public static Maybe<A> Apply<A> (this Maybe<Action<A>> mf, Maybe<A> m) {
		return mf ? new Nothing<A>() : m.FMap (mf.value);
	}

	//Apply :: Maybe (a -> b) -> Maybe a -> Maybe b
	public static Maybe<B> up<A,B> (this Maybe<Func<A,B>> mf, Maybe<A> m) {
		return mf ? new Nothing<B>() : m.FMap (mf.value);
	}
	
	//Apply :: Maybe (a -> b) -> Maybe a -> Maybe b
	public static Maybe<A> up<A> (this Maybe<Action<A>> mf, Maybe<A> m) {
		return mf ?  m.FMap (mf.value) : new Nothing<A>();
	}

	public static Maybe<B> Apply<A,B> (this Func<A,B> f, Maybe<A> m) {
		return m.FMap (f);
	}

	public static Maybe<A> Apply<A> (this Action<A> f, Maybe<A> m) {
		return m.FMap (f);
	}

	public static Maybe<B> up<A,B> (this Func<A,B> f, Maybe<A> m) {
		return m.FMap (f);
	}
	
	public static Maybe<A> up<A> (this Action<A> f, Maybe<A> m) {
		return m.FMap (f);
	}

	//Apply :: Maybe (a -> b) -> (Maybe a -> Maybe b)
	public static Func<Maybe<Func<A,B>>,Maybe<B>> Apply<A,B> (TMaybe _, Maybe<A> m) {
		return mf => mf ? new Nothing<B>() : m.FMap (mf.value);
	}

	//Apply :: Maybe (a -> void) -> (Maybe a -> Maybe a)
	public static Func<Maybe<Action<A>>,Maybe<A>> Apply<A> (TMaybe _, Maybe<A> m) {
		return mf => mf ? new Nothing<A>() : m.FMap (mf.value);
	}

}
